# BlueHarbor

## Description

BlueHarbor is a full-stack web application for the **operational management of a container terminal**. It digitalises the coordination — currently handled manually — between registering incoming vessels and planning berth usage.

> This project was developed as part of an educational programme (*Learning by Project*): the business scenario, data and rules are fictional and intended solely for learning purposes.

The application consists of:

- a **.NET (C#) backend**, structured as a Visual Studio solution (`BlueHarbor.sln`), following a layered architecture (Application / Domain / Infrastructure);
- a **React frontend**;
- a **SQL Server data layer**, managed via EF Core;
- fully **containerised execution via Docker** (Docker Compose).

### Roles

The application supports two operational roles, each user assigned to exactly one:

| Role | Responsibilities |
|------|-----------------|
| **Operator** | Registers new vessels in the system and maintains their information/status. Does not manage berth assignments. |
| **Scheduler** | Views vessels awaiting assignment (`Pending`) and assigns them to available berths according to domain rules. Handles planning decisions. |

The role is determined from the `X-Username` HTTP header (see the [Authentication](#authentication) section).

### Mock Users

| Username | Role |
|----------|------|
| `operator1` | Operator |
| `operator2` | Operator |
| `scheduler1` | Scheduler |
| `scheduler2` | Scheduler |

### Time Model

The system is **not real-time**: it maintains a **virtual current day**, advanced one unit at a time via the **Next Day** action. Hours and minutes are not tracked.

The Next Day action:
- advances the virtual day by one unit;
- updates the list of vessels;
- automatically sets the `Departed` status for vessels that have completed their occupation period (via a background **Hangfire** job);
- does **not** perform automatic assignments.

### Domain Rules

**Ship sizes**: `XL`, `L`, `M`, `S`.

**Available berths** (fixed set):

| Size | Number of berths |
|------|-----------------|
| XL   | 1               |
| L    | 1               |
| M    | 2               |
| S    | 4               |

A berth can only accommodate vessels of its own size.

**Creating a vessel** (by the Operator): the system automatically assigns a random size, a random arrival day (within 30 days from the current virtual day) and a random occupation duration (between 3 and 15 days); the Operator provides the remaining metadata (ship name, notes). The vessel is created with status `Pending`.

**Ship lifecycle**:

```
Pending  →  Assigned  →  Departed
```

- `Pending`: awaiting assignment
- `Assigned`: berth assigned
- `Departed`: occupation ended (terminal status)

**Assignment (Scheduler)**: the chosen berth must be size-compatible; the start day is the first free day of the berth; if the berth is occupied, the vessel is scheduled in the first available time slot. Upon assignment, the vessel status changes to `Assigned`.

### Out of Scope

The system must **not**: perform automatic planning or optimisations, calculate scores/KPIs, handle real-time events, model real terminals or regulations, or allow modifications/reassignments after assignment.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | C# / .NET 10, layered architecture (Application / Domain / Infrastructure) |
| Frontend | React + [Vite](https://vitejs.dev/), [Tailwind CSS](https://tailwindcss.com/) |
| Database | SQL Server 2022, data access via EF Core (`BlueHarborDbContext`) |
| Authentication | Custom header `X-Username` + Mock scheme (`MockAuthenticationHandler`) |
| Background jobs | [Hangfire](https://www.hangfire.io/) (dashboard at `/hangfire`) |
| API Docs | Native .NET 10 OpenAPI + [Scalar UI](https://scalar.com/) (`/scalar/v1`) |
| Backend tests | Dedicated project `BlueHarbor.Tests` |
| Frontend e2e tests | [Playwright](https://playwright.dev/) (`playwright.config.js`) |
| Containerisation | Docker orchestrated with `docker-compose.yml`; frontend served in production by **nginx** (`nginx.conf`) |
| IDE | JetBrains Rider / Visual Studio (`.idea` and `.DotSettings.user` files present) |

## Project Structure

```
BlueHarbor/
├── .idea/                          # JetBrains Rider configuration
├── BlueHarbor/                     # Main project (backend, .csproj)
│   ├── Application/                 # Use cases / application logic
│   │   ├── DTOs/                    # Data Transfer Objects
│   │   ├── Interfaces/              # Service and repository interfaces
│   │   ├── Security/                # MockUserDatabase + Roles constants
│   │   └── Services/                # Service implementations
│   ├── Controllers/                 # API controllers (ShipsController, SchedulerController, SystemController)
│   ├── Domain/                      # Domain entities and logic
│   ├── Infrastructure/
│   │   ├── Persistence/             # BlueHarborDbContext, DbInitializerExtensions
│   │   └── Repositories/            # Repository implementations
│   ├── Migrations/                  # EF Core migrations
│   ├── Properties/
│   │   └── launchSettings.json      # Local port: http://localhost:5151
│   ├── Security/                    # MockAuthenticationHandler (reads X-Username header)
│   ├── appsettings.json             # Connection string (LocalDB for local development)
│   ├── appsettings.Development.json
│   ├── BlueHarbor.csproj
│   ├── Create BlueHarbor.sql        # Initial database creation SQL script
│   ├── SQLQuery2.sql                # Support SQL script (seed / utility queries)
│   ├── Dockerfile
│   └── Program.cs
├── BlueHarbor.Tests/                # Backend unit/integration tests
├── frontend/                        # React client application
│   ├── dist/                        # Production build
│   ├── e2e/                         # End-to-end tests (Playwright)
│   ├── public/
│   ├── src/
│   ├── test-results/                # Playwright test output
│   ├── .env.example                 # Frontend environment variables (VITE_API_URL)
│   ├── Dockerfile
│   ├── nginx.conf                   # nginx config for serving the production build
│   ├── package.json
│   ├── playwright.config.js
│   ├── postcss.config.js
│   ├── tailwind.config.js
│   ├── vite.config.js
│   └── index.html
├── BlueHarbor.sln
├── docker-compose.yml               # Backend + frontend + DB orchestration
├── bugs.md                          # Known bug/issue tracking
└── UnitTest1.cs                     # Isolated test file in root (consider removing)
```

## Prerequisites

**To run the application (recommended):**
- Docker and Docker Compose

**For local development without Docker (optional):**
- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js v24.15.0](https://nodejs.org/) (using [nvm](https://github.com/nvm-sh/nvm) is recommended to match the version)
- SQL Server installed locally
- Recommended IDE: Visual Studio 2022 or JetBrains Rider

## Quick Start (Docker)

The application runs entirely via **Docker Compose**: no local installation of .NET SDK, Node.js or SQL Server is needed — Docker is sufficient.

### 1. Clone the repository

```bash
git clone https://github.com/fabiocerundolo-bit/BlueHarbor.git
cd BlueHarbor
```

### 2. Configure frontend environment variables

```bash
cd frontend
cp .env.example .env
cd ..
```

The `.env.example` file contains:

```env
VITE_API_URL=/api
```

`VITE_API_URL` defines the base path for API calls from the frontend. With Docker, the nginx proxy automatically routes `/api` calls to the backend.

The backend does not require a separate `.env` file: the Docker connection string is already defined directly in `docker-compose.yml` via the `ConnectionStrings__DefaultConnection` environment variable.

### 3. Build and start

```bash
docker compose up --build
```

This command starts the three services defined in `docker-compose.yml`:

| Service | Description | Exposed port |
|---------|-------------|-------------|
| `db` | SQL Server 2022 | `1433` |
| `api` | ASP.NET Core backend (.NET 10) | `8080` |
| `frontend` | React frontend served by nginx | `3001` |

On first start, the backend automatically applies EF Core migrations and seeds the database via `DbInitializerExtensions`.

**Access URLs:**

| Resource | URL |
|----------|-----|
| Application (frontend) | http://localhost:3001 |
| Backend API | http://localhost:8080/api |
| Scalar API UI (docs) | http://localhost:8080/scalar/v1 |
| Hangfire dashboard | http://localhost:8080/hangfire |

### Useful commands

```bash
# Start in background
docker compose up -d --build

# View logs
docker compose logs -f

# Stop services
docker compose down

# Stop and remove volumes (database reset)
docker compose down -v
```

## Configuration

### Backend (`appsettings.json`)

For local development the backend uses SQL Server LocalDB:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BlueHarborDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

In Docker, the connection string is overridden by the `ConnectionStrings__DefaultConnection` environment variable defined in `docker-compose.yml`.

### Frontend (`.env`)

```env
VITE_API_URL=/api
```

No additional tokens or secrets are required: authentication is handled via the `X-Username` header (see [Authentication](#authentication)).

## Local Development (without Docker)

For active development on individual parts of the project (e.g. frontend hot-reload, backend IDE debugging) both services can be run separately.

### Backend

```bash
cd BlueHarbor
dotnet restore
dotnet run
```

The API will be available at:
- `http://localhost:5151` (HTTP)
- `https://localhost:7062` (HTTPS)

The Scalar documentation will be accessible at `http://localhost:5151/scalar/v1`.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

The frontend will be available at `http://localhost:5173` (Vite default port).

### Database

The database is managed via **Entity Framework Core** (`BlueHarborDbContext`), with migrations in `BlueHarbor/Migrations/`. A dedicated SQL creation script (`Create BlueHarbor.sql`) and a utility script (`SQLQuery2.sql`) are also present for manual seed/support queries.

```bash
cd BlueHarbor
dotnet ef database update
```

On first startup, the backend automatically runs migrations and initial seeding via `DbInitializerExtensions`.

## Testing

The `BlueHarbor.Tests` project contains the automated backend tests:

```bash
cd BlueHarbor.Tests
dotnet test
```

Frontend end-to-end tests are built with **Playwright** (in the `frontend/e2e/` folder):

```bash
cd frontend
npx playwright test
```

Results are saved to `frontend/test-results/`.

## API

### Endpoints

#### Ships (`/api/ships`) — Operator + Scheduler

| Method | Endpoint | Role | Description |
|--------|----------|------|-------------|
| `GET` | `/api/ships` | Operator, Scheduler | Lists all registered vessels with assigned berth (if any) |
| `GET` | `/api/ships/{id}` | Operator | Retrieves details of a specific vessel |
| `GET` | `/api/ships/ship-list` | Operator, Scheduler | Retrieves available ship templates for creation |
| `POST` | `/api/ships` | Operator | Registers a new vessel (size, arrival and duration generated automatically) |

#### Scheduler (`/api/scheduler`) — Scheduler only

| Method | Endpoint | Role | Description |
|--------|----------|------|-------------|
| `GET` | `/api/scheduler/berths` | Scheduler | Lists all berths with their respective occupancies |
| `GET` | `/api/scheduler/pending` | Scheduler | Lists vessels in `Pending` status awaiting assignment |
| `POST` | `/api/scheduler/assign` | Scheduler | Assigns a vessel to a berth (body: `{ "shipId": int, "berthId": int }`) |

#### System (`/api/system`) — Operator + Scheduler

| Method | Endpoint | Role | Description |
|--------|----------|------|-------------|
| `GET` | `/api/system/day` | Operator, Scheduler | Returns the current virtual day |
| `POST` | `/api/system/next-day` | Operator, Scheduler | Advances the virtual day by 1 unit |

#### Other

| URL | Description |
|-----|-------------|
| `/scalar/v1` | Interactive API documentation (Scalar UI) |
| `/hangfire` | Hangfire dashboard for monitoring background jobs |

### Authentication

The backend identifies the user via a custom HTTP header, `X-Username`, included in every request instead of a JWT token or session cookie.

The header is read by `MockAuthenticationHandler`, which:
1. Extracts the value of `X-Username`;
2. Looks it up in the `MockUserDatabase` dictionary (defined in `Application/Security/SecurityModels.cs`);
3. If recognised, generates a `ClaimsPrincipal` with `ClaimTypes.Name` and `ClaimTypes.Role`;
4. If missing or unrecognised, returns `NoResult` or `Fail` respectively (→ HTTP 401/403).

**Valid users:**

| Username | Role |
|----------|------|
| `operator1` | Operator |
| `operator2` | Operator |
| `scheduler1` | Scheduler |
| `scheduler2` | Scheduler |

**Example curl call:**

```bash
curl -H "X-Username: operator1" http://localhost:8080/api/ships
```

**Frontend setup:** after login, the chosen value is attached to every HTTP request (e.g. via an Axios/fetch interceptor).

> ⚠️ **Security note**: authentication based on an arbitrary header such as `X-Username`, without a robust verification mechanism, can be easily spoofed. This approach is suitable exclusively for the educational context of this project. In a production environment, consider JWT, `HttpOnly`/`Secure` cookies, or OAuth2/OpenID Connect.

## Architectural Document (deliverable)

The project delivery requires, in addition to a working application, a brief architectural document/presentation covering:

- [ ] **Overall architecture** — partly covered by this README ([Tech Stack](#tech-stack) and [Project Structure](#project-structure) sections)
- [ ] **Main components and responsibilities** — `Application/` (services and interfaces), `Domain/` (entities), `Infrastructure/` (repositories and persistence), `Security/` (mock authentication)
- [ ] **High-level data model** — main entities: `Ship` (size, arrival day, duration, status), `Berth` (size, occupancies), `Assignment` (vessel, berth, start/end day); see `Domain/` and `Migrations/` for actual names
- [ ] **Design decisions and trade-offs** — e.g. `X-Username` header instead of standard authentication, virtual day model, simplifications relative to scope

## Contributing

1. Fork the repository
2. Create a branch for your feature (`git checkout -b feature/feature-name`)
3. Commit your changes (`git commit -m 'Add feature-name'`)
4. Push the branch (`git push origin feature/feature-name`)
5. Open a Pull Request

## Author

- [fabiocerundolo-bit](https://github.com/fabiocerundolo-bit)
