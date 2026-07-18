# BlueHarbor

## Descrizione

BlueHarbor è un'applicazione web full-stack per la **gestione operativa di un terminal container**. Digitalizza il coordinamento, oggi manuale, tra la registrazione delle navi in arrivo e la pianificazione dell'uso delle banchine.

> Il progetto nasce nell'ambito di un percorso didattico (*Learning by Project*): scenario di business, dati e regole sono fittizi e a scopo esclusivamente formativo.

L'applicazione è composta da:

- un **backend .NET (C#)**, organizzato come solution Visual Studio (`BlueHarbor.sln`), con architettura a livelli (Application / Domain / Infrastructure);
- un **frontend React**;
- uno **strato dati SQL Server**, gestito tramite EF Core;
- esecuzione **containerizzata tramite Docker** (Docker Compose).

### Ruoli

L'applicazione supporta due ruoli operativi, ciascun utente associato a uno solo:

| Ruolo | Responsabilità |
|-------|-----------------|
| **Operator** | Registra nuove navi nel sistema e ne mantiene le informazioni/stato. Non gestisce l'assegnazione delle banchine. |
| **Scheduler** | Visualizza le navi in attesa di assegnazione (`Pending`) e le assegna alle banchine disponibili secondo le regole di dominio. Gestisce le decisioni di pianificazione. |

Il ruolo viene determinato a partire dall'header HTTP `X-Username` (vedi sezione [Autenticazione](#autenticazione)).

### Utenti mock

| Username | Ruolo |
|----------|-------|
| `operator1` | Operator |
| `operator2` | Operator |
| `scheduler1` | Scheduler |
| `scheduler2` | Scheduler |

### Modello temporale

Il sistema **non è real-time**: mantiene un **giorno corrente virtuale**, avanzato di un'unità alla volta tramite l'azione **Next Day**. Non vengono gestite ore o minuti.

L'azione Next Day:
- avanza il giorno virtuale di una unità;
- aggiorna l'elenco delle navi;
- imposta automaticamente lo stato `Departed` per le navi che hanno completato il periodo di occupazione (tramite job **Hangfire** in background);
- **non** effettua assegnazioni automatiche.

### Regole di dominio

**Dimensione delle navi**: `XL`, `L`, `M`, `S`.

**Banchine disponibili** (insieme fisso):

| Dimensione | Numero banchine |
|------------|------------------|
| XL         | 1                |
| L          | 1                |
| M          | 2                |
| S          | 4                |

Una banchina può ospitare solo navi della propria dimensione.

**Creazione di una nave** (a cura dell'Operator): il sistema assegna automaticamente una dimensione casuale, un giorno di arrivo casuale (entro 30 giorni dal giorno corrente) e una durata di occupazione casuale (tra 3 e 15 giorni); l'Operator inserisce gli altri metadati (nome nave, note). La nave viene creata in stato `Pending`.

**Ciclo di vita della nave**:

```
Pending  →  Assigned  →  Departed
```

- `Pending`: in attesa di assegnazione
- `Assigned`: banchina assegnata
- `Departed`: occupazione terminata (stato conclusivo)

**Assegnazione (Scheduler)**: la banchina scelta deve essere compatibile per dimensione; il giorno di inizio è il primo giorno libero della banchina; se la banchina è occupata, la nave viene pianificata nel primo slot temporale disponibile. Al momento dell'assegnazione, lo stato della nave passa a `Assigned`.

### Fuori scope

Il sistema **non** deve: effettuare pianificazioni automatiche o ottimizzazioni, calcolare punteggi/KPI, gestire eventi real-time, modellare terminal reali o normative, consentire modifiche/riassegnazioni dopo l'assegnazione.

## Stack tecnologico

| Livello       | Tecnologia                          |
|---------------|--------------------------------------|
| Backend       | C# / .NET 10, architettura a livelli (Application / Domain / Infrastructure) |
| Frontend      | React + [Vite](https://vitejs.dev/), [Tailwind CSS](https://tailwindcss.com/) |
| Database      | SQL Server 2022, accesso dati via EF Core (`BlueHarborDbContext`) |
| Autenticazione | Header custom `X-Username` + schema Mock (`MockAuthenticationHandler`) |
| Job in background | [Hangfire](https://www.hangfire.io/) (dashboard su `/hangfire`) |
| API Docs      | Native .NET 10 OpenAPI + [Scalar UI](https://scalar.com/) (`/scalar/v1`) |
| Test backend  | Progetto dedicato `BlueHarbor.Tests` |
| Test e2e frontend | [Playwright](https://playwright.dev/) (`playwright.config.js`) |
| Containerizzazione | Docker orchestrato con `docker-compose.yml`; il frontend in produzione viene servito da **nginx** (`nginx.conf`) |
| IDE           | JetBrains Rider / Visual Studio (presenti file `.idea` e `.DotSettings.user`) |

## Struttura del progetto

```
BlueHarbor/
├── .idea/                          # Configurazione JetBrains Rider
├── BlueHarbor/                     # Progetto principale (backend, .csproj)
│   ├── Application/                 # Casi d'uso / logica applicativa
│   │   ├── DTOs/                    # Data Transfer Objects
│   │   ├── Interfaces/              # Interfacce servizi e repository
│   │   ├── Security/                # MockUserDatabase + costanti Roles
│   │   └── Services/                # Implementazioni dei servizi
│   ├── Controllers/                 # Controller API (ShipsController, SchedulerController, SystemController)
│   ├── Domain/                      # Entità e logica di dominio
│   ├── Infrastructure/
│   │   ├── Persistence/             # BlueHarborDbContext, DbInitializerExtensions
│   │   └── Repositories/            # Implementazioni dei repository
│   ├── Migrations/                  # Migrazioni EF Core
│   ├── Properties/
│   │   └── launchSettings.json      # Porta locale: http://localhost:5151
│   ├── Security/                    # MockAuthenticationHandler (lettura header X-Username)
│   ├── appsettings.json             # Connection string (LocalDB per sviluppo locale)
│   ├── appsettings.Development.json
│   ├── BlueHarbor.csproj
│   ├── Create BlueHarbor.sql        # Script SQL di creazione iniziale del database
│   ├── SQLQuery2.sql                # Script SQL di supporto (seed / query di utilità)
│   ├── Dockerfile
│   └── Program.cs
├── BlueHarbor.Tests/                # Unit/integration test backend
├── frontend/                        # Applicazione client React
│   ├── dist/                        # Build di produzione
│   ├── e2e/                         # Test end-to-end (Playwright)
│   ├── public/
│   ├── src/
│   ├── test-results/                # Output test Playwright
│   ├── .env.example                 # Variabili d'ambiente frontend (VITE_API_URL)
│   ├── Dockerfile
│   ├── nginx.conf                   # Config nginx per servire il build in produzione
│   ├── package.json
│   ├── playwright.config.js
│   ├── postcss.config.js
│   ├── tailwind.config.js
│   ├── vite.config.js
│   └── index.html
├── BlueHarbor.sln
├── docker-compose.yml               # Orchestrazione backend + frontend + DB
├── bugs.md                          # Tracking bug/issue noti
└── UnitTest1.cs                     # File di test isolato in root (da valutare se rimuovere)
```

## Prerequisiti

**Per l'esecuzione (consigliato):**
- Docker e Docker Compose

**Per lo sviluppo locale senza Docker (opzionale):**
- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js v24.15.0](https://nodejs.org/) (consigliato l'uso di [nvm](https://github.com/nvm-sh/nvm) per allinearsi alla versione)
- SQL Server installato localmente
- IDE consigliato: Visual Studio 2022 o JetBrains Rider

## Avvio rapido (Docker)

Nell'ultima versione del progetto l'esecuzione avviene interamente tramite **Docker Compose**: non è necessario installare .NET SDK, Node.js o SQL Server in locale, Docker è sufficiente.

### 1. Clonare il repository

```bash
git clone https://github.com/fabiocerundolo-bit/BlueHarbor.git
cd BlueHarbor
```

### 2. Configurare le variabili d'ambiente del frontend

```bash
cd frontend
cp .env.example .env
cd ..
```

Il file `.env.example` contiene:

```env
VITE_API_URL=/api
```

La variabile `VITE_API_URL` indica il base path delle chiamate API dal frontend. Con Docker, il proxy nginx instrada automaticamente le chiamate a `/api` verso il backend.

Il backend non richiede un file `.env` separato: la connection string per Docker è già definita direttamente nel `docker-compose.yml` tramite la variabile d'ambiente `ConnectionStrings__DefaultConnection`.

### 3. Build e avvio

```bash
docker compose up --build
```

Questo comando avvia i tre servizi definiti in `docker-compose.yml`:

| Servizio | Descrizione | Porta esposta |
|----------|-------------|---------------|
| `db` | SQL Server 2022 | `1433` |
| `api` | Backend ASP.NET Core (.NET 10) | `8080` |
| `frontend` | Frontend React servito da nginx | `3001` |

Al primo avvio il backend applica automaticamente le migrazioni EF Core e inizializza il database tramite `DbInitializerExtensions`.

**URL di accesso:**

| Risorsa | URL |
|---------|-----|
| Applicazione (frontend) | http://localhost:3001 |
| API backend | http://localhost:8080/api |
| Scalar API UI (docs) | http://localhost:8080/scalar/v1 |
| Hangfire dashboard | http://localhost:8080/hangfire |

### Comandi utili

```bash
# Avvio in background
docker compose up -d --build

# Visualizzare i log
docker compose logs -f

# Fermare i servizi
docker compose down

# Fermare e rimuovere i volumi (reset database)
docker compose down -v
```

## Configurazione

### Backend (`appsettings.json`)

Per lo sviluppo locale il backend usa SQL Server LocalDB:

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

In ambiente Docker la connection string viene sovrascritta dalla variabile d'ambiente `ConnectionStrings__DefaultConnection` definita nel `docker-compose.yml`.

### Frontend (`.env`)

```env
VITE_API_URL=/api
```

Non sono richiesti token o segreti aggiuntivi: l'autenticazione avviene tramite l'header `X-Username` (vedi sezione [Autenticazione](#autenticazione)).

## Sviluppo locale (senza Docker)

Per lo sviluppo attivo su singole parti del progetto (es. hot-reload del frontend, debug del backend in IDE) è possibile eseguire i due servizi separatamente.

### Backend

```bash
cd BlueHarbor
dotnet restore
dotnet run
```

L'API sarà disponibile su:
- `http://localhost:5151` (HTTP)
- `https://localhost:7062` (HTTPS)

La documentazione Scalar sarà accessibile su `http://localhost:5151/scalar/v1`.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Il frontend sarà disponibile su `http://localhost:5173` (porta Vite di default).

### Database

Il database è gestito tramite **Entity Framework Core** (`BlueHarborDbContext`), con migrazioni in `BlueHarbor/Migrations/`. È inoltre presente uno script SQL di creazione dedicato (`Create BlueHarbor.sql`) e uno script di utilità (`SQLQuery2.sql`) per query di supporto/seed manuale.

```bash
cd BlueHarbor
dotnet ef database update
```

Il backend, al primo avvio, esegue automaticamente le migrazioni e il seeding iniziale tramite `DbInitializerExtensions`.

## Test

Il progetto `BlueHarbor.Tests` contiene i test automatici del backend:

```bash
cd BlueHarbor.Tests
dotnet test
```

I test end-to-end del frontend sono realizzati con **Playwright** (cartella `frontend/e2e/`):

```bash
cd frontend
npx playwright test
```

I risultati vengono salvati in `frontend/test-results/`.

## API

### Endpoints

#### Ships (`/api/ships`) — Operator + Scheduler

| Metodo | Endpoint | Ruolo | Descrizione |
|--------|----------|-------|-------------|
| `GET` | `/api/ships` | Operator, Scheduler | Elenca tutte le navi registrate con banchina assegnata (se presente) |
| `GET` | `/api/ships/{id}` | Operator | Recupera i dettagli di una nave specifica |
| `GET` | `/api/ships/ship-list` | Operator, Scheduler | Recupera i template di navi disponibili per la creazione |
| `POST` | `/api/ships` | Operator | Registra una nuova nave (dimensione, arrivo e durata generati automaticamente) |

#### Scheduler (`/api/scheduler`) — solo Scheduler

| Metodo | Endpoint | Ruolo | Descrizione |
|--------|----------|-------|-------------|
| `GET` | `/api/scheduler/berths` | Scheduler | Elenca tutte le banchine con le rispettive occupazioni |
| `GET` | `/api/scheduler/pending` | Scheduler | Elenca le navi in stato `Pending` in attesa di assegnazione |
| `POST` | `/api/scheduler/assign` | Scheduler | Assegna una nave a una banchina (body: `{ "shipId": int, "berthId": int }`) |

#### System (`/api/system`) — Operator + Scheduler

| Metodo | Endpoint | Ruolo | Descrizione |
|--------|----------|-------|-------------|
| `GET` | `/api/system/day` | Operator, Scheduler | Restituisce il giorno virtuale corrente |
| `POST` | `/api/system/next-day` | Operator, Scheduler | Avanza il giorno virtuale di 1 unità |

#### Altri

| URL | Descrizione |
|-----|-------------|
| `/scalar/v1` | Documentazione interattiva dell'API (Scalar UI) |
| `/hangfire` | Dashboard Hangfire per monitorare i job in background |

### Autenticazione

Il backend identifica l'utente tramite un header HTTP custom, `X-Username`, incluso in ogni richiesta al posto di un token JWT o di un cookie di sessione.

L'header viene letto da `MockAuthenticationHandler`, che:
1. Estrae il valore di `X-Username`;
2. Lo confronta con il dizionario `MockUserDatabase` (definito in `Application/Security/SecurityModels.cs`);
3. Se riconosciuto, genera un `ClaimsPrincipal` con `ClaimTypes.Name` e `ClaimTypes.Role`;
4. Se assente o non riconosciuto, restituisce rispettivamente `NoResult` o `Fail` (→ HTTP 401/403).

**Utenti validi:**

| Username | Ruolo |
|----------|-------|
| `operator1` | Operator |
| `operator2` | Operator |
| `scheduler1` | Scheduler |
| `scheduler2` | Scheduler |

**Esempio di chiamata curl:**

```bash
curl -H "X-Username: operator1" http://localhost:8080/api/ships
```

**Impostazione lato frontend:** dopo il login, il valore scelto viene allegato a ogni richiesta HTTP (es. tramite interceptor Axios/fetch).

> ⚠️ **Nota di sicurezza**: un'autenticazione basata su un header arbitrario come `X-Username`, se non accompagnata da un meccanismo di verifica robusto, può essere facilmente falsificata. Questa soluzione è adatta esclusivamente all'ambiente didattico del progetto. In un contesto reale, valutare JWT, cookie `HttpOnly`/`Secure` o OAuth2/OpenID Connect.

## Documento architetturale (deliverable)

La consegna del progetto richiede, oltre all'applicazione funzionante, un breve documento/presentazione architetturale che copra:

- [ ] **Architettura complessiva** — già in parte coperta da questo README (sezione [Stack tecnologico](#stack-tecnologico) e [Struttura del progetto](#struttura-del-progetto))
- [ ] **Componenti principali e responsabilità** — `Application/` (servizi e interfacce), `Domain/` (entità), `Infrastructure/` (repository e persistenza), `Security/` (autenticazione mock)
- [ ] **Modello dati ad alto livello** — entità principali: `Ship` (dimensione, giorno di arrivo, durata, stato), `Berth` (dimensione, occupazioni), `Assignment` (nave, banchina, giorno inizio/fine); si vedano `Domain/` ed `Migrations/` per i nomi reali
- [ ] **Decisioni progettuali e compromessi** — es. header `X-Username` invece di autenticazione standard, modello a giorno virtuale, semplificazioni rispetto allo scope

## Contribuire

1. Crea un fork del repository
2. Crea un branch per la tua feature (`git checkout -b feature/nome-feature`)
3. Effettua il commit delle modifiche (`git commit -m 'Aggiunge nome-feature'`)
4. Fai push del branch (`git push origin feature/nome-feature`)
5. Apri una Pull Request

## Autore

- [fabiocerundolo-bit](https://github.com/fabiocerundolo-bit)
