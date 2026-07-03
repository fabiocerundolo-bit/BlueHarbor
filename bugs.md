# Bug Analysis - BlueHarbor

In line with the instructions received, the detected issues were only analyzed and documented, without making changes or optimizations to the source code. The following is the detailed list of problems found in the system.

---

## 1. Security Test Failure (`Security_Scheduler_CannotCreateShip`)
* **Reference file**: [IntegrationTests.cs](file:///c:/Users/fabio.cerundolo/Documents/BlueHarbor-masterv01/BlueHarbor/BlueHarbor.Tests/IntegrationTests.cs#L133-L154)
* **Details**:
  The `Security_Scheduler_CannotCreateShip` integration test fails consistently. The test retrieves the `[Authorize]` attribute at the `ShipsController` class level and performs an exact equality check against the `Operator` role:
  ```csharp
  Assert.Equal(Roles.Operator, authorizeAttr.Roles);
  ```
  However, the `ShipsController` class is globally decorated with:
  ```csharp
  [Authorize(Roles = Roles.Operator + "," + Roles.Scheduler)]
  ```
  because the `GetAllShips` endpoint must be accessible to both roles. The specific restriction for ship creation is correctly defined only at the `CreateShip` method level. For this reason, the test fails (it compares `"Operator"` with `"Operator,Scheduler"`).

---

## 2. Retroactive Ship Assignment in the Past (Scheduling Algorithm)
* **Reference file**: [SchedulerService.cs](file:///c:/Users/fabio.cerundolo/Documents/BlueHarbor-masterv01/BlueHarbor/BlueHarbor/Application/Services/SchedulerService.cs#L61) in the `AssignShipToBerthAsync` method
* **Details**:
  The time-slot planning logic for docking starts the search from the ship's originally recorded arrival day:
  ```csharp
  int startDay = FindFirstAvailableSlot(berth, ship.GiornoArrivo, ship.DurataOccupazione);
  ```
  If the operator advances harbor time (moving the virtual day forward through the "Next Day" action) without immediately scheduling the pending ship, the current day (`CurrentDay`) becomes greater than the ship's arrival day (`ship.GiornoArrivo`).
  In that situation, the algorithm calculates the slot based on a past date and retroactively assigns the ship to a day earlier than the harbor's current day. The initial slot calculation should be based on `Math.Max(ship.GiornoArrivo, currentDay)`.

---

## 3. Role-Switch Race Condition in the Frontend (React Context)
* **Reference file**: [AppContext.jsx](file:///c:/Users/fabio.cerundolo/Documents/BlueHarbor-masterv01/BlueHarbor/frontend/src/context/AppContext.jsx#L143-L153) in the `setRole` callback
* **Details**:
  Inside `setRole(r)`, the React state is updated through `setRoleState(r)`. Immediately after that, `refreshShips()` is invoked synchronously to load ships:
  ```javascript
  if (r === 'Operator') refreshShips()
  ```
  However, because React state updates are asynchronous, `refreshShips` runs while `roleRef.current` (or `role`) still points to the previous role (for example, `'Scheduler'`). As a result, the client sends the HTTP request to `/api/ships` with the mock header `X-Username: scheduler1` instead of `operator1`.

---

## 4. Overly Strict Berth Size Constraint
* **Reference file**: [SchedulerService.cs](file:///c:/Users/fabio.cerundolo/Documents/BlueHarbor-masterv01/BlueHarbor/BlueHarbor/Application/Services/SchedulerService.cs#L55-L58)
* **Details**:
  The ship-to-berth compatibility logic enforces an exact equality check on the size identifier:
  ```csharp
  if (berth.IdDimensione != ship.IdDimensione)
  {
      throw new InvalidOperationException(...);
  }
  ```
  In a real harbor scenario, smaller ships can dock at larger berths (for example, a `Small` ship in a `Medium`, `Large`, or `XL` berth). Enforcing exact equality (`!=`) drastically limits berth utilization and prevents smaller ships from using available larger spaces.

---

## 5. Fragile Timeout Dependency for Hangfire Consistency (Design Smell / Race Condition)
* **Reference file**: [AppContext.jsx](file:///c:/Users/fabio.cerundolo/Documents/BlueHarbor-masterv01/BlueHarbor/frontend/src/context/AppContext.jsx#L121-L124) in the `doAdvanceDay` callback
* **Details**:
  When the user advances the current harbor day, the `/api/system/next-day` HTTP request asynchronously queues a Hangfire background job to mark departed ships as `"Departed"`.
  To compensate for this asynchronous processing, the frontend inserts a forced 700ms delay before reloading data:
  ```javascript
  await new Promise(resolve => setTimeout(resolve, 700))
  ```
  This introduces a latent race condition: if the server slows down or the database is busy, the Hangfire job may take longer than 700ms to complete. As a result, the frontend fetches data before the database state is updated and keeps showing ships as `"Assigned"` instead of `"Departed"`.

---

## 6. Database Name Mismatch in the Setup Flow
* **Reference files**: [appsettings.json](file:///c:/Users/fabio.cerundolo/Documents/BlueHarbor-masterv01/BlueHarbor/BlueHarbor/appsettings.json#L3) and [Create BlueHarbor.sql](file:///c:/Users/fabio.cerundolo/Documents/BlueHarbor-masterv01/BlueHarbor/BlueHarbor/Create%20BlueHarbor.sql#L1)
* **Details**:
  The default connection string configured in `appsettings.json` points to a database named `BlueHarborDb`:
  ```json
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BlueHarborDb;..."
  ```
  However, the manual initialization SQL script `Create BlueHarbor.sql` starts with:
  ```sql
  CREATE DATABASE BlueHarbor;
  ```
  This naming mismatch can confuse the system administrator or accidentally lead to two separate databases coexisting on local instances, causing manually created data to drift from the data managed by the Entity Framework ORM.
