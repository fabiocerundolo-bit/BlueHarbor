# Analisi dei Bug Rilevati - BlueHarbor

In conformità con le istruzioni ricevute, i bug rilevati sono stati esclusivamente analizzati e documentati, senza apportare modifiche o ottimizzazioni al codice sorgente. Di seguito è riportato l'elenco dettagliato dei problemi riscontrati nel sistema.

---

## 1. Fallimento del Test di Sicurezza (`Security_Scheduler_CannotCreateShip`)
* **File di riferimento**: [IntegrationTests.cs](file:///c:/Users/fabio.cerundolo/Documents/BlueHarbor-masterv01/BlueHarbor/BlueHarbor.Tests/IntegrationTests.cs#L133-L154)
* **Dettagli**: 
  Il test d'integrazione `Security_Scheduler_CannotCreateShip` fallisce sistematicamente. Il test recupera l'attributo `[Authorize]` a livello di classe `ShipsController` ed esegue una verifica di uguaglianza esatta con il ruolo `Operatore`:
  ```csharp
  Assert.Equal(Roles.Operatore, authorizeAttr.Roles);
  ```
  Tuttavia, la classe `ShipsController` è decorata a livello globale con:
  ```csharp
  [Authorize(Roles = Roles.Operatore + "," + Roles.Scheduler)]
  ```
  poiché l'endpoint `GetAllShips` deve essere accessibile a entrambi i ruoli. La restrizione specifica per la creazione è definita correttamente solo a livello del singolo metodo `CreateShip`. Per questo motivo il test fallisce (confronta `"Operatore"` con `"Operatore,Scheduler"`).

---

## 2. Assegnazione Retroattiva delle Navi nel Passato (Algoritmo di Scheduling)
* **File di riferimento**: [SchedulerService.cs](file:///c:/Users/fabio.cerundolo/Documents/BlueHarbor-masterv01/BlueHarbor/BlueHarbor/Application/Services/SchedulerService.cs#L61) nel metodo `AssignShipToBerthAsync`
* **Dettagli**:
  La pianificazione dello slot temporale per l'attracco invoca la funzione di ricerca a partire dal giorno di arrivo originario registrato per la nave:
  ```csharp
  int startDay = FindFirstAvailableSlot(berth, ship.GiornoArrivo, ship.DurataOccupazione);
  ```
  Se l'operatore fa scorrere il tempo del porto (avanzando il giorno virtuale tramite l'azione "Next Day") senza pianificare immediatamente la nave in attesa, il giorno corrente (`CurrentDay`) diventerà maggiore del giorno di arrivo della nave (`ship.GiornoArrivo`). 
  In questa situazione, l'algoritmo calcolerà lo slot basandosi su una data passata e assegnerà retroattivamente la nave a un giorno precedente a quello attuale del porto. Il calcolo dello slot iniziale dovrebbe basarsi su `Math.Max(ship.GiornoArrivo, currentDay)`.

---

## 3. Race Condition al Cambio Ruolo nel Frontend (React Context)
* **File di riferimento**: [AppContext.jsx](file:///c:/Users/fabio.cerundolo/Documents/BlueHarbor-masterv01/BlueHarbor/frontend/src/context/AppContext.jsx#L143-L153) nella callback `setRole`
* **Dettagli**:
  All'interno di `setRole(r)`, viene eseguita la mutazione dello stato React tramite `setRoleState(r)`. Subito dopo, viene invocato `refreshShips()` in modo sincrono per caricare le navi:
  ```javascript
  if (r === 'Operatore') refreshShips()
  ```
  Tuttavia, poiché gli aggiornamenti dello stato React sono asincroni, la funzione `refreshShips` viene invocata quando la variabile `roleRef.current` (o `role`) fa ancora riferimento al vecchio ruolo (es. `'Scheduler'`). Di conseguenza, il client effettua la chiamata HTTP `/api/ships` inviando l'header mock `X-Username: scheduler1` invece di `operatore1`.

---

## 4. Vincolo Eccessivamente Rigido di Dimensione delle Banchine
* **File di riferimento**: [SchedulerService.cs](file:///c:/Users/fabio.cerundolo/Documents/BlueHarbor-masterv01/BlueHarbor/BlueHarbor/Application/Services/SchedulerService.cs#L55-L58)
* **Dettagli**:
  La logica di compatibilità tra navi e banchine impone un controllo di uguaglianza esatto sull'identificativo della dimensione:
  ```csharp
  if (berth.IdDimensione != ship.IdDimensione)
  {
      throw new InvalidOperationException(...);
  }
  ```
  In uno scenario portuale reale, le navi di dimensioni inferiori possono attraccare in banchine più grandi (ad esempio una nave `Small` in una banchina `Medium`, `Large` o `XL`). L'imposizione del vincolo di uguaglianza esatta (`!=`) limita drasticamente l'allocazione delle risorse portuali e non permette alle navi piccole di sfruttare gli spazi più grandi vuoti.

---

## 5. Dipendenza Fragile dal Timeout per la Consistenza di Hangfire (Design Smell / Race Condition)
* **File di riferimento**: [AppContext.jsx](file:///c:/Users/fabio.cerundolo/Documents/BlueHarbor-masterv01/BlueHarbor/frontend/src/context/AppContext.jsx#L121-L124) nella callback `doAdvanceDay`
* **Dettagli**:
  Quando l'utente avanza il giorno corrente nel porto, la richiesta HTTP `/api/system/next-day` accoda in modo asincrono un job Hangfire in background per impostare le navi salpate come `"Departed"`. 
  Il frontend, per compensare questa elaborazione asincrona, implementa un ritardo forzato di 700ms prima di ricaricare i dati:
  ```javascript
  await new Promise(resolve => setTimeout(resolve, 700))
  ```
  Questa soluzione introduce una race condition latente: se il server subisce rallentamenti o se il database è congestionato, il job di Hangfire potrebbe impiegare più di 700ms per completarsi. Di conseguenza, il frontend effettuerà il fetch dei dati prima che lo stato sia aggiornato a database, continuando a mostrare le navi come `"Assigned"` anziché `"Departed"`.

---

## 6. Disallineamento nei Nomi del Database nel Flusso di Setup
* **File di riferimento**: [appsettings.json](file:///c:/Users/fabio.cerundolo/Documents/BlueHarbor-masterv01/BlueHarbor/BlueHarbor/appsettings.json#L3) e [Create BlueHarbor.sql](file:///c:/Users/fabio.cerundolo/Documents/BlueHarbor-masterv01/BlueHarbor/BlueHarbor/Create%20BlueHarbor.sql#L1)
* **Dettagli**:
  La stringa di connessione predefinita configurata in `appsettings.json` punta a un database denominato `BlueHarborDb`:
  ```json
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BlueHarborDb;..."
  ```
  Tuttavia, lo script SQL di inizializzazione manuale `Create BlueHarbor.sql` inizia con l'istruzione:
  ```sql
  CREATE DATABASE BlueHarbor;
  ```
  Questa discrepanza di nomi può indurre in errore l'amministratore del sistema o causare la coesistenza accidentale di due database separati su istanze locali, disallineando i dati creati manualmente rispetto a quelli gestiti dall'ORM Entity Framework.
