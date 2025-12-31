# Sistema di Autenticazione e Multi-Partita - Implementazione Completata

## 📋 Panoramica

Questo documento descrive l'implementazione del nuovo sistema di autenticazione tradizionale e della funzionalità multi-partita per l'applicazione FantaSottone.

## ✅ Modifiche Implementate

### 1. **Schema Database**

#### Modifiche Esistenti (Già Supportate)

Lo schema database esistente **supporta già** il requisito multi-partita:

- `UserEntity` contiene i dati utente (Username, PasswordHash, Email)
- `PlayerEntity` collega `UserId` a `GameId` permettendo partecipazione multipla
- Constraint `UX_PlayerEntity_GameId_UserId` previene duplicati

#### Ottimizzazioni Aggiunte

**File**: [`docs/db-migration-001-auth-improvements.sql`](docs/db-migration-001-auth-improvements.sql)

```sql
-- Indice per query utente → partite
CREATE NONCLUSTERED INDEX [IX_PlayerEntity_UserId]
ON [dbo].[PlayerEntity]([UserId] ASC)
INCLUDE ([GameId], [IsCreator], [CurrentScore], [CreatedAt], [UpdatedAt]);

-- Indice per ordinamento dashboard
CREATE NONCLUSTERED INDEX [IX_GameEntity_UpdatedAt]
ON [dbo].[GameEntity]([UpdatedAt] DESC);
```

**Eseguire lo script**: Applicare il migration script al database prima di utilizzare le nuove funzionalità.

---

### 2. **Domain Layer - Modelli e DTOs**

#### Nuovi Modelli

- **[`User.cs`](src/Domains/Internal.FantaSottone.Domain/Models/User.cs)**: Rappresenta un utente registrato
- **[`Player.cs`](src/Domains/Internal.FantaSottone.Domain/Models/Player.cs)**: Aggiunto campo `UserId` per collegamento con User

#### Nuovi DTOs

Tutti i DTOs sono nella cartella [`Domain/Dtos/`](src/Domains/Internal.FantaSottone.Domain/Dtos/):

| File                     | Descrizione                                        |
| ------------------------ | -------------------------------------------------- |
| `RegisterRequest.cs`     | Dati per registrazione (Username, Password, Email) |
| `LoginRequest.cs`        | Dati per login (Username, Password)                |
| `LoginResponse.cs`       | Risposta login con token JWT                       |
| `GameListItemDto.cs`     | Item per dashboard utente con info partita         |
| `InvitePlayerRequest.cs` | Richiesta invito giocatore                         |
| `JoinGameRequest.cs`     | Richiesta join partita                             |
| `UserSearchDto.cs`       | Risultato ricerca utenti                           |

---

### 3. **Domain Layer - Servizi e Repository**

#### Nuovi Servizi

- **[`IAuthService`](src/Domains/Internal.FantaSottone.Domain/Services/IAuthService.cs)**: Autenticazione, registrazione, hash password
- **[`IUserService`](src/Domains/Internal.FantaSottone.Domain/Services/IUserService.cs)**: CRUD utenti, ricerca, lista partite utente

#### Servizi Aggiornati

- **[`IGameService`](src/Domains/Internal.FantaSottone.Domain/Services/IGameService.cs)**: Aggiunti metodi `InvitePlayerAsync` e `JoinGameAsync`

#### Nuovi Repository

- **[`IUserRepository`](src/Domains/Internal.FantaSottone.Domain/Repositories/IUserRepository.cs)**: Gestione dati utenti con query ottimizzate
  - `GetByUsernameAsync`: Ricerca per username
  - `SearchUsersAsync`: Ricerca pattern per inviti
  - `GetUserGamesAsync`: Recupera lista partite con JOIN ottimizzati

#### Repository Aggiornati

- **[`IGameRepository`](src/Domains/Internal.FantaSottone.Domain/Repositories/IGameRepository.cs)**: Aggiunto `IsUserCreatorAsync`
- **[`IPlayerRepository`](src/Domains/Internal.FantaSottone.Domain/Repositories/IPlayerRepository.cs)**: Aggiunti `GetByGameAndUserAsync`, `ExistsAsync`

---

### 4. **Business Layer - Implementazioni**

#### Nuovi Servizi

- **[`AuthService`](src/Businesses/Internal.FantaSottone.Business/Services/AuthService.cs)**:
  - Hash password con SHA256
  - Generazione token (semplificato, da sostituire con JWT in produzione)
  - Validazione credenziali
- **[`UserService`](src/Businesses/Internal.FantaSottone.Business/Services/UserService.cs)**:
  - Operazioni CRUD su utenti
  - Ricerca utenti per inviti
  - Recupero partite utente con dettagli

#### Servizi Aggiornati

- **[`GameService`](src/Businesses/Internal.FantaSottone.Business/Services/GameService.cs)**:
  - `InvitePlayerAsync`: Validazione creatore, controllo duplicati, creazione PlayerEntity
  - `JoinGameAsync`: Verifica appartenenza utente alla partita

---

### 5. **Infrastructure Layer - Repositories**

#### Nuovi Repository

- **[`UserRepository`](src/Infrastructures/Internal.FantaSottone.Infrastructure/Repositories/UserRepository.cs)**:
  - Query EF Core ottimizzate con Include/ThenInclude
  - `GetUserGamesAsync` con JOIN su PlayerEntity → GameEntity → CreatorPlayer → User
  - Mapping automatico status byte → string

#### Repository Aggiornati

- **[`GameRepository`](src/Infrastructures/Internal.FantaSottone.Infrastructure/Repositories/GameRepository.cs)**: Metodo `IsUserCreatorAsync` con navigazione CreatorPlayer
- **[`PlayerRepository`](src/Infrastructures/Internal.FantaSottone.Infrastructure/Repositories/PlayerRepository.cs)**: Query per game+user combo

#### Mapper Aggiornati

- **[`EntityMapper`](src/Infrastructures/Internal.FantaSottone.Infrastructure/Mappers/EntityMapper.cs)**:
  - Mapping User ↔ UserEntity
  - Player aggiornato con `UserId`

---

### 6. **API Layer - Controllers**

#### Controller Aggiornati

**[`AuthController`](src/Apis/Internal.FantaSottone.Api/Controllers/AuthController.cs)**

- `POST /api/auth/register`: Registrazione nuovo utente + auto-login
- `POST /api/auth/login`: Login con username/password, ritorna token JWT

#### Nuovi Controller

**[`UsersController`](src/Apis/Internal.FantaSottone.Api/Controllers/UsersController.cs)**

- `GET /api/users/me/games`: Dashboard partite utente (con header `X-User-Id`)
- `GET /api/users/search?query={term}`: Ricerca utenti per inviti

**[`GamesController`](src/Apis/Internal.FantaSottone.Api/Controllers/GamesController.cs)** - Nuovi Endpoint

- `POST /api/games/{gameId}/invite`: Invita utente registrato (solo creatore)
- `POST /api/games/{gameId}/join`: Join partita e recupera contesto

---

## 🔌 API Endpoints Reference

### Autenticazione

#### Registrazione Utente

```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "mario.rossi",
  "password": "SecureP@ss123",
  "email": "mario.rossi@example.com"
}
```

**Response (201 Created)**

```json
{
  "userId": 1,
  "username": "mario.rossi",
  "email": "mario.rossi@example.com",
  "token": "base64encodedtoken..."
}
```

#### Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "mario.rossi",
  "password": "SecureP@ss123"
}
```

**Response (200 OK)**

```json
{
  "userId": 1,
  "username": "mario.rossi",
  "email": "mario.rossi@example.com",
  "token": "base64encodedtoken..."
}
```

---

### Dashboard Utente

#### Recupera Lista Partite

```http
GET /api/users/me/games
X-User-Id: 1
```

**Response (200 OK)**

```json
[
  {
    "gameId": 5,
    "gameName": "Partita del Venerdì",
    "creatorUsername": "luca.bianchi",
    "status": 2,
    "statusText": "InProgress",
    "currentScore": 85,
    "isCreator": false,
    "updatedAt": "2025-12-28T18:30:00Z"
  },
  {
    "gameId": 3,
    "gameName": "Torneo Epico",
    "creatorUsername": "mario.rossi",
    "status": 1,
    "statusText": "Pending",
    "currentScore": 100,
    "isCreator": true,
    "updatedAt": "2025-12-27T10:00:00Z"
  }
]
```

**Status Mapping**

- `1` = Pending
- `2` = InProgress
- `3` = Ended

---

### Ricerca Utenti

#### Cerca Utenti per Inviti

```http
GET /api/users/search?query=mario
```

**Response (200 OK)**

```json
[
  {
    "userId": 1,
    "username": "mario.rossi",
    "email": "mario.rossi@example.com"
  },
  {
    "userId": 12,
    "username": "mario.verdi",
    "email": "mario.verdi@example.com"
  }
]
```

---

### Gestione Partite

#### Invita Giocatore (Solo Creatore)

```http
POST /api/games/5/invite
X-User-Id: 1
Content-Type: application/json

{
  "gameId": 5,
  "userId": 12
}
```

**Response (200 OK)**

```json
{
  "message": "Player invited successfully",
  "playerId": 23
}
```

**Errori Comuni**

- `403 Forbidden`: Richiedente non è il creatore
- `404 Not Found`: Game o User non esistono
- `409 Conflict`: Utente già nella partita

#### Join Partita

```http
POST /api/games/5/join
X-User-Id: 1
```

**Response (200 OK)**

```json
{
  "message": "Joined game successfully",
  "game": {
    "id": 5,
    "name": "Partita del Venerdì",
    "status": 2,
    "initialScore": 100
  },
  "player": {
    "id": 18,
    "currentScore": 85,
    "isCreator": false
  }
}
```

**Errori Comuni**

- `403 Forbidden`: Utente non è un giocatore di questa partita
- `404 Not Found`: Partita non esiste

---

## 🔐 Autenticazione

### Implementazione Attuale (Temporanea)

Attualmente l'autenticazione usa l'header `X-User-Id` per semplicità:

```javascript
fetch("/api/users/me/games", {
  headers: {
    "X-User-Id": "1",
  },
});
```

### ⚠️ TODO: Implementazione JWT (Produzione)

**File da modificare**:

1. **[`AuthService.cs`](src/Businesses/Internal.FantaSottone.Business/Services/AuthService.cs)**:

   - Sostituire `GenerateToken` con generazione JWT
   - Aggiungere claims: `sub` (userId), `name` (username), `email`

2. **[`Program.cs`](src/Apis/Internal.FantaSottone.Api/Program.cs)**:

   - Configurare `AddJwtBearer` con secret key
   - Configurare validazione token

3. **Controllers**:
   - Sostituire `X-User-Id` header con `User.FindFirst(ClaimTypes.NameIdentifier)`
   - Aggiungere `[Authorize]` attribute

**Esempio JWT**:

```csharp
// In AuthService.cs
private string GenerateToken(User user)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]);
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        }),
        Expires = DateTime.UtcNow.AddHours(24),
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}
```

---

## 🎨 Frontend - Implementazione UI

### 1. Dashboard Partite

**Componente**: `GameDashboard.tsx`

```tsx
interface GameListItem {
  gameId: number;
  gameName: string;
  creatorUsername: string;
  status: number;
  statusText: string;
  currentScore: number;
  isCreator: boolean;
  updatedAt: string;
}

function GameDashboard() {
  const [games, setGames] = useState<GameListItem[]>([]);
  const userId = getCurrentUserId(); // From auth context

  useEffect(() => {
    fetch("/api/users/me/games", {
      headers: { "X-User-Id": userId.toString() },
    })
      .then((res) => res.json())
      .then(setGames);
  }, [userId]);

  return (
    <div className="game-dashboard">
      <h1>Le Tue Partite</h1>
      {games.map((game) => (
        <GameCard
          key={game.gameId}
          game={game}
          onJoin={() => joinGame(game.gameId)}
        />
      ))}
    </div>
  );
}
```

**Card Partita**:

```tsx
function GameCard({
  game,
  onJoin,
}: {
  game: GameListItem;
  onJoin: () => void;
}) {
  const statusColor = {
    1: "bg-yellow-100",
    2: "bg-green-100",
    3: "bg-gray-100",
  }[game.status];

  return (
    <div className={`game-card ${statusColor} p-4 rounded-lg mb-4`}>
      <div className="flex justify-between items-start">
        <div>
          <h2 className="text-xl font-bold">{game.gameName}</h2>
          <p className="text-sm text-gray-600">
            Creatore: {game.creatorUsername}
          </p>
        </div>
        <div className="text-right">
          <span className={`badge badge-${game.statusText.toLowerCase()}`}>
            {game.statusText}
          </span>
          {game.isCreator && <span className="ml-2">👑</span>}
        </div>
      </div>
      <div className="mt-2 flex justify-between items-center">
        <span className="text-lg">
          Punteggio: <strong>{game.currentScore}</strong>
        </span>
        <button onClick={onJoin} className="btn btn-primary">
          Entra →
        </button>
      </div>
    </div>
  );
}
```

---

### 2. Sistema Inviti

**Componente**: `InvitePlayerModal.tsx`

```tsx
function InvitePlayerModal({ gameId, onClose }: Props) {
  const [searchQuery, setSearchQuery] = useState("");
  const [users, setUsers] = useState<UserSearchDto[]>([]);
  const userId = getCurrentUserId();

  const searchUsers = async (query: string) => {
    if (query.length < 2) return;

    const res = await fetch(
      `/api/users/search?query=${encodeURIComponent(query)}`
    );
    const data = await res.json();
    setUsers(data);
  };

  const inviteUser = async (targetUserId: number) => {
    const res = await fetch(`/api/games/${gameId}/invite`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "X-User-Id": userId.toString(),
      },
      body: JSON.stringify({ gameId, userId: targetUserId }),
    });

    if (res.ok) {
      alert("Giocatore invitato con successo!");
      onClose();
    } else {
      const error = await res.json();
      alert(error.title || "Errore durante l'invito");
    }
  };

  return (
    <div className="modal">
      <h2>Invita Giocatore</h2>

      <input
        type="text"
        placeholder="Cerca utente..."
        value={searchQuery}
        onChange={(e) => {
          setSearchQuery(e.target.value);
          searchUsers(e.target.value);
        }}
        className="search-input"
      />

      <div className="user-list">
        {users.map((user) => (
          <div key={user.userId} className="user-item">
            <div>
              <strong>{user.username}</strong>
              <span className="text-sm text-gray-600">{user.email}</span>
            </div>
            <button onClick={() => inviteUser(user.userId)} className="btn-sm">
              Invita
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}
```

---

### 3. Registrazione e Login

**Componente**: `Register.tsx`

```tsx
function Register() {
  const [formData, setFormData] = useState({
    username: "",
    password: "",
    email: "",
  });

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();

    const res = await fetch("/api/auth/register", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(formData),
    });

    if (res.ok) {
      const data = await res.json();
      // Save token and userId
      localStorage.setItem("authToken", data.token);
      localStorage.setItem("userId", data.userId);
      // Redirect to dashboard
      window.location.href = "/dashboard";
    } else {
      const error = await res.json();
      alert(error.title);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <input
        type="text"
        placeholder="Username"
        value={formData.username}
        onChange={(e) => setFormData({ ...formData, username: e.target.value })}
        required
      />
      <input
        type="email"
        placeholder="Email"
        value={formData.email}
        onChange={(e) => setFormData({ ...formData, email: e.target.value })}
        required
      />
      <input
        type="password"
        placeholder="Password"
        value={formData.password}
        onChange={(e) => setFormData({ ...formData, password: e.target.value })}
        required
        minLength={6}
      />
      <button type="submit">Registrati</button>
    </form>
  );
}
```

---

## 🔄 Flusso Utente

### Scenario 1: Nuovo Utente

1. **Registrazione**: `POST /api/auth/register`

   - Sistema crea UserEntity
   - Auto-login ritorna token
   - Redirect a dashboard (vuota, nessuna partita)

2. **Creazione Partita** (futuro):

   - Utente crea partita → diventa `CreatorPlayerId`
   - Sistema crea GameEntity + PlayerEntity (IsCreator=true)

3. **Invito Giocatori**:
   - Creatore cerca utenti: `GET /api/users/search?query=mario`
   - Invita: `POST /api/games/{id}/invite` → crea PlayerEntity per invitato
   - Invitato vede partita nella sua dashboard

### Scenario 2: Giocatore Invitato

1. **Login**: `POST /api/auth/login`
2. **Dashboard**: `GET /api/users/me/games`
   - Vede partite dove è PlayerEntity
3. **Join Partita**: Click su card → `POST /api/games/{id}/join`
   - Verifica appartenenza
   - Imposta come "partita attiva"
   - Redirect a schermata di gioco

### Scenario 3: Multi-Partita

- Utente può essere PlayerEntity in più GameEntity
- Ogni partita ha ruolo indipendente (creatore in una, giocatore normale in altre)
- Dashboard mostra tutte con badge "👑" per quelle dove è creatore

---

## 📊 Regole di Validazione

### Registrazione

- **Username**: Obbligatorio, unico, max 50 caratteri
- **Password**: Min 6 caratteri (da migliorare con requisiti complessità)
- **Email**: Obbligatorio, formato valido, max 100 caratteri

### Invito Giocatori

- Solo il creatore può invitare (`GameEntity.CreatorPlayerId == requesting UserId`)
- Utente deve esistere in `UserEntity`
- Non può invitare stesso utente due volte (constraint `UX_PlayerEntity_GameId_UserId`)
- Partita deve esistere

### Join Partita

- Utente deve avere un `PlayerEntity` per quel `GameId`
- Partita deve esistere

---

## 🚀 Deploy e Testing

### 1. Applicare Migration Database

```bash
# Connettiti al database SQL Server
sqlcmd -S <server> -d sqldb-fantastn-webapp-dev -U <user> -P <password>

# Esegui lo script
:r docs/db-migration-001-auth-improvements.sql
GO
```

### 2. Compilare e Testare Backend

```bash
cd src/Apis/Internal.FantaSottone.Api
dotnet build
dotnet run
```

### 3. Test API con Postman/curl

**Registrazione**:

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"test.user","password":"Test123!","email":"test@example.com"}'
```

**Login**:

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"test.user","password":"Test123!"}'
```

**Dashboard** (usa userId dalla risposta login):

```bash
curl http://localhost:5000/api/users/me/games \
  -H "X-User-Id: 1"
```

---

## ⚠️ Note Importanti

### Sicurezza

1. **Password Hashing**: Attualmente usa SHA256. Per produzione usare **bcrypt** o **Argon2**

   ```bash
   dotnet add package BCrypt.Net-Next
   ```

2. **JWT Token**: Implementare JWT vero invece del token semplificato

   ```bash
   dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
   ```

3. **HTTPS**: Forzare HTTPS in produzione

### Performance

- Gli indici `IX_PlayerEntity_UserId` e `IX_GameEntity_UpdatedAt` sono essenziali
- Query dashboard usa Include/ThenInclude ottimizzati
- Considerare caching per lista partite utente (Redis)

### Compatibilità

- Sistema **mantiene compatibilità** con vecchio meccanismo (AccessCode ancora nel Player model)
- Migrazione graduale possibile: nuovi utenti con sistema nuovo, vecchi con AccessCode

---

## 📝 Changelog Completo

### Database

- ✅ Aggiunto indice `IX_PlayerEntity_UserId`
- ✅ Aggiunto indice `IX_GameEntity_UpdatedAt`

### Domain Layer

- ✅ Creato `User` model
- ✅ Creati 7 DTOs (Register, Login, GameList, Invite, Join, UserSearch)
- ✅ Creato `IAuthService`, `IUserService`
- ✅ Creato `IUserRepository`
- ✅ Aggiornati `IGameService`, `IGameRepository`, `IPlayerRepository`

### Business Layer

- ✅ Implementato `AuthService` (hash, token, validazione)
- ✅ Implementato `UserService` (CRUD, search, games)
- ✅ Aggiornato `GameService` (invite, join)
- ✅ Registrazione servizi in DI container

### Infrastructure Layer

- ✅ Implementato `UserRepository` con query ottimizzate
- ✅ Aggiornati `GameRepository`, `PlayerRepository`
- ✅ Aggiornato `EntityMapper` per User
- ✅ Registrazione repository in DI container

### API Layer

- ✅ Aggiornato `AuthController` (register, login)
- ✅ Creato `UsersController` (dashboard, search)
- ✅ Aggiornato `GamesController` (invite, join)

---

## 🎯 Prossimi Passi Consigliati

1. **Implementare JWT** (vedi sezione Autenticazione)
2. **Migliorare hash password** (bcrypt)
3. **Frontend React**:
   - Dashboard component
   - Modal inviti
   - Form registrazione/login
4. **Testing**:
   - Unit test per servizi
   - Integration test per repository
   - E2E test per flusso completo
5. **Validazione Input**:
   - FluentValidation per DTOs
   - Regex per email
   - Requisiti complessità password
6. **Gestione Errori**:
   - Middleware global error handler
   - Logging strutturato (Serilog)
7. **Documentazione API**:
   - Swagger/OpenAPI spec
   - Esempi request/response

---

## 📞 Contatti e Supporto

Per domande sull'implementazione o problemi:

1. Verificare log applicazione
2. Controllare vincoli database (constraint violations)
3. Testare endpoint con Postman
4. Consultare questo documento

**Buon lavoro! 🚀**
