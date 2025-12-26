---
agent: agent
---

Sei un senior backend engineer ASP.NET Core (Clean Architecture / DDD pragmatico) e devi implementare il backend di **FantaSottone** a partire da uno schema DB-first già definito e da una lista di endpoint.

## Contesto (DB-First)

Lo schema SQL Server del database è il seguente (NON modificarlo nel codice, ma puoi proporre modifiche allo schema se necessarie):

```sql
/* =========================================================
   FantaSottone - DB First schema
   Fix: avoid multiple cascade paths on RuleAssignmentEntity
   ========================================================= */

IF OBJECT_ID('dbo.RuleAssignmentEntity', 'U') IS NOT NULL DROP TABLE dbo.RuleAssignmentEntity;
IF OBJECT_ID('dbo.RuleEntity', 'U') IS NOT NULL DROP TABLE dbo.RuleEntity;
IF OBJECT_ID('dbo.PlayerEntity', 'U') IS NOT NULL DROP TABLE dbo.PlayerEntity;
IF OBJECT_ID('dbo.GameEntity', 'U') IS NOT NULL DROP TABLE dbo.GameEntity;
GO

/* =======================
   GameEntity
   ======================= */
CREATE TABLE dbo.GameEntity
(
    Id                  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_GameEntity PRIMARY KEY,
    Name                NVARCHAR(100) NOT NULL,
    InitialScore        INT NOT NULL,
    Status              TINYINT NOT NULL, -- 1=Draft, 2=Started, 3=Ended
    CreatorPlayerId     INT NULL,
    WinnerPlayerId      INT NULL,
    CreatedAt           DATETIME2(3) NOT NULL CONSTRAINT DF_GameEntity_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt           DATETIME2(3) NOT NULL CONSTRAINT DF_GameEntity_UpdatedAt DEFAULT SYSUTCDATETIME()
);
GO

/* =======================
   PlayerEntity
   ======================= */
CREATE TABLE dbo.PlayerEntity
(
    Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PlayerEntity PRIMARY KEY,
    GameId          INT NOT NULL,
    Username        NVARCHAR(50) NOT NULL,
    AccessCode      NVARCHAR(32) NOT NULL,
    IsCreator       BIT NOT NULL CONSTRAINT DF_PlayerEntity_IsCreator DEFAULT (0),
    CurrentScore    INT NOT NULL,
    CreatedAt       DATETIME2(3) NOT NULL CONSTRAINT DF_PlayerEntity_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2(3) NOT NULL CONSTRAINT DF_PlayerEntity_UpdatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE dbo.PlayerEntity
ADD CONSTRAINT FK_PlayerEntity_GameEntity
FOREIGN KEY (GameId) REFERENCES dbo.GameEntity(Id)
ON DELETE CASCADE;
GO

CREATE UNIQUE INDEX UX_PlayerEntity_GameId_Username
ON dbo.PlayerEntity(GameId, Username);

CREATE UNIQUE INDEX UX_PlayerEntity_GameId_AccessCode
ON dbo.PlayerEntity(GameId, AccessCode);
GO

/* =======================
   RuleEntity
   ======================= */
CREATE TABLE dbo.RuleEntity
(
    Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RuleEntity PRIMARY KEY,
    GameId          INT NOT NULL,
    Name            NVARCHAR(100) NOT NULL,
    RuleType        TINYINT NOT NULL, -- 1=Bonus, 2=Malus
    ScoreDelta      INT NOT NULL,
    CreatedAt       DATETIME2(3) NOT NULL CONSTRAINT DF_RuleEntity_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2(3) NOT NULL CONSTRAINT DF_RuleEntity_UpdatedAt DEFAULT SYSUTCDATETIME()
);
GO

ALTER TABLE dbo.RuleEntity
ADD CONSTRAINT FK_RuleEntity_GameEntity
FOREIGN KEY (GameId) REFERENCES dbo.GameEntity(Id)
ON DELETE CASCADE;
GO

CREATE UNIQUE INDEX UX_RuleEntity_GameId_Name
ON dbo.RuleEntity(GameId, Name);
GO

/* =======================
   RuleAssignmentEntity
   ======================= */
CREATE TABLE dbo.RuleAssignmentEntity
(
    Id                  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RuleAssignmentEntity PRIMARY KEY,
    RuleId              INT NOT NULL,
    GameId              INT NOT NULL,
    AssignedToPlayerId  INT NOT NULL,
    ScoreDeltaApplied   INT NOT NULL,
    AssignedAt          DATETIME2(3) NOT NULL CONSTRAINT DF_RuleAssignmentEntity_AssignedAt DEFAULT SYSUTCDATETIME(),
    CreatedAt           DATETIME2(3) NOT NULL CONSTRAINT DF_RuleAssignmentEntity_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt           DATETIME2(3) NOT NULL CONSTRAINT DF_RuleAssignmentEntity_UpdatedAt DEFAULT SYSUTCDATETIME()
);
GO

-- Cascade qui va bene: se elimino una regola, elimino le sue assegnazioni.
ALTER TABLE dbo.RuleAssignmentEntity
ADD CONSTRAINT FK_RuleAssignmentEntity_RuleEntity
FOREIGN KEY (RuleId) REFERENCES dbo.RuleEntity(Id)
ON DELETE CASCADE;
GO

-- NO ACTION (default): evita catene di cascade multiple verso RuleAssignmentEntity
ALTER TABLE dbo.RuleAssignmentEntity
ADD CONSTRAINT FK_RuleAssignmentEntity_PlayerEntity
FOREIGN KEY (AssignedToPlayerId) REFERENCES dbo.PlayerEntity(Id);
GO

-- NO ACTION (default): evita percorso diretto di cascade Game -> Assignment
ALTER TABLE dbo.RuleAssignmentEntity
ADD CONSTRAINT FK_RuleAssignmentEntity_GameEntity
FOREIGN KEY (GameId) REFERENCES dbo.GameEntity(Id);
GO

-- Meccanismo "La prima che": una regola assegnabile una sola volta
CREATE UNIQUE INDEX UX_RuleAssignmentEntity_RuleId
ON dbo.RuleAssignmentEntity(RuleId);
GO

CREATE INDEX IX_RuleAssignmentEntity_GameId_AssignedAt
ON dbo.RuleAssignmentEntity(GameId, AssignedAt DESC);

CREATE INDEX IX_PlayerEntity_GameId_CurrentScore
ON dbo.PlayerEntity(GameId, CurrentScore DESC);
GO

/* =======================
   FK circolari su GameEntity
   ======================= */
ALTER TABLE dbo.GameEntity
ADD CONSTRAINT FK_GameEntity_CreatorPlayer
FOREIGN KEY (CreatorPlayerId) REFERENCES dbo.PlayerEntity(Id);

ALTER TABLE dbo.GameEntity
ADD CONSTRAINT FK_GameEntity_WinnerPlayer
FOREIGN KEY (WinnerPlayerId) REFERENCES dbo.PlayerEntity(Id);
GO

/* =======================
   CHECK constraints
   ======================= */
ALTER TABLE dbo.GameEntity
ADD CONSTRAINT CK_GameEntity_Status
CHECK (Status IN (1,2,3));

ALTER TABLE dbo.RuleEntity
ADD CONSTRAINT CK_RuleEntity_RuleType
CHECK (RuleType IN (1,2));
GO
```

## Endpoint richiesti (contratto API)

Implementare i seguenti endpoint con le route, request/response e codici errore esattamente come descritti:

# Lista Endpoint API - FantaSottone Backend

Sulla base della documentazione fornita, ecco gli endpoint da sviluppare per il backend ASP.NET Core:

---

## **1. AUTENTICAZIONE**

### `POST /api/auth/login`

**Descrizione:** Autentica un giocatore con username e access code.
**Request:**

```json
{
  "username": "string",
  "accessCode": "string"
}
```

**Response (200):**

```json
{
  "token": "string (JWT)",
  "game": {
    "Id": "number",
    "Name": "string",
    "InitialScore": "number",
    "Status": "number (1=Draft, 2=Started, 3=Ended)",
    "CreatorPlayerId": "number | null",
    "WinnerPlayerId": "number | null"
  },
  "player": {
    "Id": "number",
    "GameId": "number",
    "Username": "string",
    "IsCreator": "boolean",
    "CurrentScore": "number"
  }
}
```

**Errori:** 401 se credenziali invalide

---

## **2. SETUP PARTITA** (solo Creator)

### `POST /api/games/start`

**Descrizione:** Crea una nuova partita con giocatori e regole. Transisce lo stato da Draft a Started.  
**Autorizzazione:** Creator  
**Request:**

```json
{
  "name": "string",
  "initialScore": "number",
  "players": [
    {
      "username": "string",
      "accessCode": "string",
      "isCreator": "boolean (opzionale, default false per non-primo)"
    }
  ],
  "rules": [
    {
      "name": "string",
      "ruleType": "number (1=Bonus, 2=Malus)",
      "scoreDelta": "number (positivo o negativo)"
    }
  ]
}
```

**Response (200):**

```json
{
  "gameId": "number",
  "credentials": [
    {
      "username": "string",
      "accessCode": "string",
      "isCreator": "boolean"
    }
  ]
}
```

**Errori:** 400 se dati invalidi, 403 se non creator

---

## **3. RUNTIME PARTITA**

### `GET /api/games/{gameId}/leaderboard`

**Descrizione:** Recupera la classifica della partita ordinata per punteggio (decrescente).  
**Response (200):**

```json
[
  {
    "Id": "number",
    "Username": "string",
    "CurrentScore": "number",
    "IsCreator": "boolean"
  }
]
```

**Errori:** 404 se game non esiste

---

### `GET /api/games/{gameId}/rules`

**Descrizione:** Recupera la lista di regole con lo stato di assegnazione (se assegnata a chi e quando).  
**Response (200):**

```json
[
  {
    "rule": {
      "Id": "number",
      "Name": "string",
      "RuleType": "number (1=Bonus, 2=Malus)",
      "ScoreDelta": "number"
    },
    "assignment": {
      "ruleAssignmentId": "number",
      "assignedToPlayerId": "number",
      "assignedToUsername": "string",
      "assignedAt": "string (ISO date)"
    } | null
  }
]
```

**Errori:** 404 se game non esiste

---

### `POST /api/games/{gameId}/rules/{ruleId}/assign`

**Descrizione:** Assegna una regola al giocatore autenticato. Atomico: se due richieste arrivano insieme, una vince (409 conflict). Aggiorna il punteggio del giocatore con il delta della regola.  
**Autorizzazione:** Autenticato  
**Request:**

```json
{
  "playerId": "number"
}
```

**Response (200):**

```json
{
  "assignment": {
    "id": "number",
    "ruleId": "number",
    "assignedToPlayerId": "number",
    "assignedAt": "string (ISO date)",
    "scoreDeltaApplied": "number"
  },
  "updatedPlayer": {
    "Id": "number",
    "CurrentScore": "number"
  },
  "gameStatus": {
    "status": "number (2=Started, 3=Ended)",
    "winnerPlayerId": "number | null"
  }
}
```

**Errori:**

- 404 se rule o game non esiste
- **409 Conflict** se la regola è già assegnata ("La prima che")
- Trigger end game se: tutte le regole assegnate OR ≥3 giocatori con score ≤0

---

### `GET /api/games/{gameId}/status`

**Descrizione:** Recupera lo stato attuale della partita e il vincitore (se terminata).  
**Response (200):**

```json
{
  "game": {
    "Id": "number",
    "Status": "number (2=Started, 3=Ended)",
    "WinnerPlayerId": "number | null"
  },
  "winner": {
    "Id": "number",
    "Username": "string",
    "CurrentScore": "number"
  } | null
}
```

**Errori:** 404 se game non esiste

---

### `GET /api/games/{gameId}/assignments`

**Descrizione:** Recupera lo storico completo di tutte le assegnazioni di regole (audit trail).  
**Response (200):**

```json
[
  {
    "id": "number",
    "ruleId": "number",
    "ruleName": "string",
    "assignedToPlayerId": "number",
    "assignedToUsername": "string",
    "scoreDeltaApplied": "number",
    "assignedAt": "string (ISO date)"
  }
]
```

**Errori:** 404 se game non esiste

---

## **4. TERMINAZIONE PARTITA** (solo Creator)

### `POST /api/games/{gameId}/end`

**Descrizione:** Termina manualmente una partita. Il vincitore è determinato dal punteggio più alto (in caso di parità: giocatore con ID più basso o chi ha raggiunto per primo quel punteggio - scegliere una regola deterministica).  
**Autorizzazione:** Creator della partita  
**Request:** (body vuoto)  
**Response (200):**

```json
{
  "game": {
    "Id": "number",
    "Status": "3 (Ended)",
    "WinnerPlayerId": "number"
  },
  "winner": {
    "Id": "number",
    "Username": "string",
    "CurrentScore": "number"
  },
  "leaderboard": [
    {
      "Id": "number",
      "Username": "string",
      "CurrentScore": "number",
      "IsCreator": "boolean"
    }
  ]
}
```

**Errori:**

- 404 se game non esiste
- 403 se l'autenticato non è il creator
- 400 se game è già terminata

---

## **5. MODIFICA REGOLE** (solo Creator, pre-assegnazione)

### `PUT /api/games/{gameId}/rules/{ruleId}`

**Descrizione:** Modifica una regola che non è ancora stata assegnata. Una volta assegnata, non può più essere modificata.  
**Autorizzazione:** Creator della partita  
**Request:**

```json
{
  "name": "string",
  "ruleType": "number (1=Bonus, 2=Malus)",
  "scoreDelta": "number"
}
```

**Response (200):**

```json
{
  "rule": {
    "Id": "number",
    "Name": "string",
    "RuleType": "number",
    "ScoreDelta": "number"
  }
}
```

**Errori:**

- 404 se rule o game non esiste
- 403 se non creator
- **409 Conflict** se la regola è già stata assegnata

---

## **LINEE GUIDA IMPLEMENTATIVE**

| Aspetto            | Dettaglio                                                                                                         |
| ------------------ | ----------------------------------------------------------------------------------------------------------------- |
| **Concorrenza**    | Assign rule: transazione + unique constraint su `RuleAssignmentEntity.RuleId` oppure serializable isolation level |
| **Vincoli DB**     | `PlayerEntity`: unique (GameId, Username), unique (GameId, AccessCode); `RuleAssignmentEntity`: unique (RuleId)   |
| **Autorizzazione** | Usare [Authorize] con JWT; validare che PlayerId nel token corrisponda a game                                     |
| **Validazione**    | FluentValidation facoltativa ma consigliata                                                                       |
| **Error Handling** | 409 Conflict deve essere distinguibile (type/code specifico) dal client                                           |
| **Osservabilità**  | Application Insights per telemetry; log strutturato con ILogger                                                   |

---

Questi **7 endpoint** (+ il facoltativo PUT per modificare rules) rappresentano il minimo indispensabile per far funzionare l'app completa.

```

---

# Obiettivo

Produrre una soluzione backend pronta per essere integrata in un progetto ASP.NET Core con:

- Domain models aggiornati
- Validator aggiornati
- Repository + Service layer (generici + specifici)
- DTO per request/response degli endpoint
- Controller/Minimal API endpoint implementati secondo le route indicate
- Gestione concorrenza e error handling coerenti con i requisiti (409, 403, 404, 400, 401)

---

# Vincoli architetturali (OBBLIGATORI)

1. **Dipendenze**

   - Controller → Manager/Service (mai repository)
   - Manager → Service
   - Service → Repository
   - Repository NON si conoscono tra loro

2. **Dependency Injection**

   - Nel costruttore si inietta sempre l’interfaccia necessaria.
   - I servizi e i repository devono essere registrati tramite **ServiceCollection extensions**.

3. **Generici + specifici**

   - Creare prima:
     - `IRepository<TEntity, TKey>`
     - `IService<TEntity, TKey>`
   - Poi creare le interfacce specifiche, es.:
     - `IGameRepository : IRepository<GameEntity, int>`
     - `IGameService : IService<GameEntity, int>`
   - Stessa logica per Player, Rule, RuleAssignment.

4. **BaseRepository**

   - Poiché è DB-first, creare un `BaseRepository<TEntity, TKey>` astratto che implementa la logica comune CRUD / query di base usando DbContext (o altro approccio coerente col progetto).
   - I repository concreti estendono BaseRepository e aggiungono query specifiche.

5. **Concorrenza sull’assegnazione regole**
   - Endpoint `POST /api/games/{gameId}/rules/{ruleId}/assign` deve essere **atomico**:
     - se la regola è già assegnata → **409 Conflict**
     - usare transazione + vincolo unico `UX_RuleAssignmentEntity_RuleId`
     - oppure isolamento `SERIALIZABLE` (specificare quale scegli e perché)
   - Gestire correttamente race condition.

---

# Modalità di esecuzione (STEP OBBLIGATORI)

## Step 1 — Verifica DB (STOP & WAIT)

1. Analizza lo schema DB e confrontalo con i requisiti degli endpoint.
2. Elenca eventuali **modifiche consigliate** al DB (vincoli, indici, colonne, FK, ecc.), specificando:
   - motivazione
   - impatto sugli endpoint
   - migrazione necessaria (se applicabile)
3. **Fermati qui** e chiedimi l’OK per procedere.

> Nota: NON implementare nulla oltre Step 1 finché non ricevi l’OK.

---

## Step 2 — Domain Models

Aggiorna/crea i modelli di dominio coerenti con DB e use case:

- entità + relazioni + enum (Status, RuleType)
- eventuali value objects se utili (solo se apportano valore reale)

## Step 3 — Validators

Aggiorna/crea i validator (es. FluentValidation) per:

- Login
- Start game
- Assign rule
- Update rule
- Qualsiasi input con vincoli non banali (range, required, uniqueness lato app quando utile)

## Step 4 — Service Interfaces

Crea:

- `IService<TEntity, TKey>`
- interfacce specifiche per ogni entità
- (se utile) manager interfaces per orchestrazione per use-case (es. `IGameManager`, `IAuthManager`)

## Step 5 — Repository Interfaces + Implementazioni

Crea:

- `IRepository<TEntity, TKey>`
- `BaseRepository<TEntity, TKey>` astratto
- repository specifici (Game/Player/Rule/RuleAssignment) con metodi query necessari agli endpoint:
  - leaderboard
  - rules con assignment info
  - assignment audit trail
  - start/end game transitions
  - check winner / end conditions

## Step 6 — DTO (Request/Response)

Crea DTO separati dal dominio:

- Request DTO per ogni endpoint
- Response DTO esattamente come contratto (nomi campi inclusi)
- mapping (manuale o AutoMapper — specifica scelta)

## Step 7 — Endpoint Implementation

Crea e implementa gli endpoint secondo le route:

- Auth (JWT)
- Setup partita
- Runtime (leaderboard, rules, assign, status, assignments)
- End manuale
- (Opzionale) Update rule

Con:

- `[Authorize]` dove richiesto
- validazione player/game coerente col token
- error handling standardizzato (ProblemDetails consigliato)
- codici 400/401/403/404/409 esatti

---

# Output atteso per ciascuno step

Per ogni step produci:

- elenco file/classi da creare/modificare
- snippet di codice completo per le parti principali
- note tecniche su decisioni (concorrenza, transazioni, mapping, error handling)
- eventuali TODO espliciti se manca contesto, ma minimizzarli

---

Inizia dallo **Step 1 (Verifica DB)** e poi fermati.
```
