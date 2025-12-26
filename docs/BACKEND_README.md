# FantaSottone Backend - Implementazione Completata

## 📋 Panoramica

Backend ASP.NET Core 8.0 implementato seguendo **Clean Architecture / DDD pragmatico** con approccio **DB-First**.

---

## 🏗️ Architettura

```
src/
├── Domains/                    # Domain layer (no dependencies)
│   └── Internal.FantaSottone.Domain/
│       ├── Models/             # Domain models (Game, Player, Rule, RuleAssignment)
│       ├── Services/           # Service interfaces
│       ├── Repositories/       # Repository interfaces
│       ├── Managers/           # Manager interfaces (orchestrazione)
│       └── Results/            # AppResult pattern, Error, StatusCode
│
├── Infrastructures/            # Infrastructure layer (depends: Domain)
│   └── Internal.FantaSottone.Infrastructure/
│       ├── Models/             # EF entities (auto-generated DB-first)
│       ├── Repositories/       # Repository implementations
│       └── Mappers/            # Entity ↔ Domain mapping
│
├── Businesses/                 # Business layer (depends: Domain, Infrastructure)
│   └── Internal.FantaSottone.Business/
│       ├── Services/           # Service implementations
│       ├── Managers/           # Manager implementations
│       └── Validators/         # FluentValidation validators
│
└── Apis/                       # API layer (depends: Business)
    └── Internal.FantaSottone.Api/
        ├── Controllers/        # API controllers
        └── DTOs/              # Request/Response DTOs
```

---

## 🔑 Features Implementate

### ✅ Endpoint Completi

1. **POST /api/auth/login** - Autenticazione JWT
2. **POST /api/games/start** - Creazione partita con giocatori e regole
3. **GET /api/games/{gameId}/leaderboard** - Classifica giocatori
4. **GET /api/games/{gameId}/rules** - Regole con stato assegnazione
5. **POST /api/games/{gameId}/rules/{ruleId}/assign** - Assegnazione regola (atomic)
6. **GET /api/games/{gameId}/status** - Stato partita e vincitore
7. **GET /api/games/{gameId}/assignments** - Audit trail assegnazioni
8. **POST /api/games/{gameId}/end** - Fine partita manuale (creator only)
9. **PUT /api/games/{gameId}/rules/{ruleId}** - Modifica regola (pre-assegnazione)

### ✅ Funzionalità Chiave

- **JWT Authentication** con claim playerId/gameId/isCreator
- **Concurrency handling** su assegnazione regole (unique constraint + DbUpdateException catch)
- **Auto-end game** quando:
  - Tutte le regole assegnate
  - ≥3 giocatori con score ≤0
- **Tie-break vincitore**: punteggio più alto → Id ASC
- **Error handling**: ProblemDetails con codici HTTP corretti (400/401/403/404/409)
- **Validazione RuleType/ScoreDelta** (Fix #3): Bonus > 0, Malus < 0

---

## 🗄️ Database

### Schema SQL Server

Tabelle principali:

- `GameEntity` (Status: 1=Draft, 2=Started, 3=Ended)
- `PlayerEntity` (unique per GameId+Username e GameId+AccessCode)
- `RuleEntity` (RuleType: 1=Bonus, 2=Malus)
- `RuleAssignmentEntity` (unique per RuleId - "La prima che")

### Modifiche Applicate

✅ **Fix #2**: Index `IX_RuleEntity_GameId` (performance `/rules`)  
✅ **Fix #5**: Index `IX_RuleAssignmentEntity_GameId_RuleId`  
✅ **Fix #6**: Index `IX_GameEntity_Status`  
⚠️ **Fix #1**: CASCADE su `FK_RuleAssignmentEntity_GameEntity` **NON applicato** (circular dependency) - rimasto NO ACTION

### Connection String

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=FantaSottone;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

---

## 🔐 Configurazione JWT

In `appsettings.json`:

```json
"Jwt": {
  "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
  "Issuer": "FantaSottone",
  "Audience": "FantaSottone",
  "ExpiryMinutes": "480"
}
```

**⚠️ IMPORTANTE**: Cambia `SecretKey` in produzione!

---

## 🚀 Quick Start

### 1. Prerequisiti

- .NET 8.0 SDK
- SQL Server (LocalDB o instance)
- Database `FantaSottone` creato e schema applicato

### 2. Applicare lo schema DB

Esegui lo script SQL fornito (con modifiche indici):

```sql
-- Esegui lo script SQL completo fornito nella documentazione
-- Include tabelle + indici + vincoli
```

### 3. Configurare Connection String

Modifica `appsettings.Development.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=FantaSottone;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 4. Eseguire l'applicazione

```bash
cd src/Apis/Internal.FantaSottone.Api
dotnet run
```

Swagger UI: `https://localhost:5001/swagger`

---

## 📝 Flusso d'uso tipico

### 1. Creare una partita

```http
POST /api/games/start
Content-Type: application/json

{
  "name": "Partita Test",
  "initialScore": 100,
  "players": [
    { "username": "alice", "accessCode": "abc123", "isCreator": true },
    { "username": "bob", "accessCode": "xyz789", "isCreator": false }
  ],
  "rules": [
    { "name": "Drink Shot", "ruleType": 2, "scoreDelta": -10 },
    { "name": "Win Hand", "ruleType": 1, "scoreDelta": 5 }
  ]
}
```

**Response**: `{ "gameId": 1, "credentials": [...] }`

### 2. Login giocatore

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "alice",
  "accessCode": "abc123"
}
```

**Response**: JWT token + game + player info

### 3. Assegnare regola

```http
POST /api/games/1/rules/1/assign
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json

{
  "playerId": 1
}
```

**Response**: assignment + updatedPlayer + gameStatus (auto-end se condizioni)

### 4. Terminare partita

```http
POST /api/games/1/end
Authorization: Bearer {JWT_TOKEN}
```

**Response**: game + winner + leaderboard

---

## 🧪 Testing

### Swagger UI

1. Avvia l'app in Development
2. Vai a `https://localhost:5001/swagger`
3. Testa `/api/auth/login` per ottenere token
4. Clicca "Authorize" e inserisci: `Bearer {token}`
5. Testa gli altri endpoint autenticati

### Postman Collection

Importa gli endpoint da Swagger JSON:

- `https://localhost:5001/swagger/v1/swagger.json`

---

## 🏛️ Design Decisions

### Dependency Injection Rules (OBBLIGATORI)

✅ **Controller → Manager/Service** (mai repository diretto)  
✅ **Manager → Service** (orchestrazione use-case complessi)  
✅ **Service → Repository** (business logic)  
✅ **Repository NON si conoscono tra loro**

### AppResult Pattern

```csharp
AppResult<T> // success + value
AppResult    // success/fail senza valore

// Factory methods
AppResult.Success()
AppResult<T>.Success(data)
AppResult.NotFound(message)
AppResult<T>.Conflict(message, code)
```

### Concorrenza su Assign Rule

**Strategia**: Unique constraint + catch `DbUpdateException`

```csharp
try {
    await _ruleAssignmentRepository.AddAsync(assignment);
    await SaveChangesAsync();
}
catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UX_RuleAssignmentEntity_RuleId")) {
    return AppResult<T>.Conflict("Rule already assigned", "RULE_ALREADY_ASSIGNED");
}
```

**Alternativa non usata**: Isolation Level SERIALIZABLE (più lento)

### Mapping Entity ↔ Domain

Extension methods in `EntityMapper.cs`:

```csharp
GameEntity.ToDomain() → Game
Game.ToEntity() → GameEntity
```

---

## ⚠️ Known Issues / TODO

1. **Fix #1 non applicato**: NO ACTION su `FK_RuleAssignmentEntity_GameEntity` causa impossibilità eliminazione game con assignments. Gestire manualmente se necessario.

2. **Validazione pre-save**: Validators FluentValidation non invocati automaticamente, chiamare manualmente se necessario

3. **Soft delete**: Non implementato, eliminazione fisica

4. **Pagination**: Non implementata su leaderboard/assignments (OK per MVP)

5. **Caching**: Non implementato (Redis consigliato per prod)

---

## 📦 Dipendenze Principali

```xml
<!-- Domain -->
- Nessuna dipendenza esterna

<!-- Infrastructure -->
- Microsoft.EntityFrameworkCore.SqlServer 8.0.20

<!-- Business -->
- FluentValidation 12.1.1
- System.IdentityModel.Tokens.Jwt 8.4.0

<!-- API -->
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.11
- Swashbuckle.AspNetCore 6.5.0
```

---

## 👨‍💻 Note Tecniche

### BaseModel.Id

Cambiato da `{ get; }` a `{ get; set; }` per permettere mapping EF → Domain

### Tuple in AppResult

Tuple non sono `class`, quindi creati wrapper:

- `StartGameResult`
- `LoginResult`

### DateTime UTC

Tutti i DateTime sono generati con `DateTime.UtcNow` per consistenza

---

## 📚 Risorse

- **Swagger**: `https://localhost:5001/swagger`
- **Health Check**: `https://localhost:5001/health`
- **Connection String**: `appsettings.json`

---

**Implementazione completata il**: 2025-12-25  
**Versione**: 1.0  
**Framework**: .NET 8.0
