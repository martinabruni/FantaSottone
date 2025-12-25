---
agent: agent
---

Agisci come un senior frontend engineer. Sviluppa il frontend dell’app “FantaSottone” usando:

- React 18+
- TypeScript
- Vite
- shadcn/ui come UNICA libreria di componenti UI (nessun altro kit componenti)
- Tailwind (necessario per shadcn)
- Tema dark/light con toggle e persistenza (localStorage)
- Nessun codice duplicato/copia-incolla: estrai componenti, hook e utility riusabili.

REQUISITI UI/UX (AGGIUNTIVI E OBBLIGATORI)

1. Responsive-first

- L’app deve essere completamente responsive (mobile-first).
- Landing page: su mobile le due colonne diventano stack verticali (Login sopra, Creazione sotto).
- Tabelle/classifiche: su mobile devono restare leggibili (wrap intelligente, o layout “card list” quando necessario).
- Header e Tabs devono adattarsi correttamente a schermi piccoli.

2. Lingua

- Tutte le label, i testi UI, bottoni, titoli, placeholder e messaggi devono essere in italiano.
- Eccezioni: nomi tecnici di file/cartelle o tipi TypeScript.

3. Feedback visivi e pattern colore azioni

- Tutti i pulsanti devono avere feedback visivi chiari: hover, active, focus-visible (accessibilità), disabled.
- I pulsanti devono essere colorati anche in base al tipo di azione usando pattern:
  - info (azioni neutre/consultazione)
  - success (azioni conferma/creazione/avvio)
  - warning (azioni potenzialmente rischiose/modifica rilevante)
  - error (azioni distruttive/termine partita)
- Implementa una piccola utility/variant system centralizzato (es. `buttonVariants` o wrapper `ActionButton`) per evitare duplicazioni e mantenere coerenza cromatica.

4. Toast/Alert (fix richiesto)

- Gli alert/toast NON devono avere background trasparente.
- Usa Toaster di shadcn: imposta classi e varianti per garantire background pieno e coerente con info/success/warning/error.
- Ogni toast deve avere titolo e descrizione in italiano quando opportuno.

CONTESTO (MODELLO DATI - DB FIRST)
Le entità persistite nel database (e quindi la base per i DTO) sono:

1. GameEntity

   - Id: number
   - Name: string
   - InitialScore: number
   - Status: number (1=Draft, 2=Started, 3=Ended)
   - CreatorPlayerId?: number | null
   - WinnerPlayerId?: number | null
   - CreatedAt: string (ISO)
   - UpdatedAt: string (ISO)

2. PlayerEntity

   - Id: number
   - GameId: number
   - Username: string
   - AccessCode: string
   - IsCreator: boolean
   - CurrentScore: number
   - CreatedAt: string (ISO)
   - UpdatedAt: string (ISO)

3. RuleEntity

   - Id: number
   - GameId: number
   - Name: string
   - RuleType: number (1=Bonus, 2=Malus)
   - ScoreDelta: number (positivo/negativo)
   - CreatedAt: string (ISO)
   - UpdatedAt: string (ISO)

4. RuleAssignmentEntity
   - Id: number
   - RuleId: number
   - GameId: number
   - AssignedToPlayerId: number
   - ScoreDeltaApplied: number
   - AssignedAt: string (ISO)
   - CreatedAt: string (ISO)
   - UpdatedAt: string (ISO)

Nota: “La prima che” è garantito dal vincolo UNIQUE su RuleAssignmentEntity.RuleId nel backend.
Il frontend deve gestire 409 Conflict mostrando un messaggio (toast) e refreshando lo stato.

OBIETTIVO FUNZIONALE

- Landing page divisa in due colonne:
  A) Accesso: Username + AccessCode
  B) Creazione partita: Name, InitialScore, gestione lista players e rules, “Avvia partita”
- Dopo login: pagina partita con tab:
  - Tab “Classifica”: lista PlayerEntity ordinata per CurrentScore desc, evidenzia player corrente
  - Tab “Regole”: lista RuleEntity con stato assegnabile/assegnata; bottone “Assegna a me”
    - Se già assegnata: disabilita e mostra “Assegnata da <username>” e timestamp (AssignedAt)
- Fine partita:
  - mostra status (Bozza/In corso/Terminata) e se Terminata mostra Vincitore (PlayerEntity) e classifica finale.

NUOVE REGOLE DI GIOCO (AGGIUNTE OBBLIGATORIE)

A) Il creatore può terminare la partita in qualsiasi momento

- Una volta avviata una partita (Status=Started), il creatore deve vedere un’azione “Termina partita”.
- Quando il creatore termina la partita, viene decretato il vincitore.
- Regola vincitore: è il player con CurrentScore più alto al momento della terminazione.
  - In caso di pari merito, applica un criterio deterministico (es. vince quello con Id minore) e mostrane la logica in un tooltip o messaggio (in italiano).
- UI:
  - “Termina partita” deve essere un’azione di tipo **error** (distruttiva) e richiedere conferma via Dialog (shadcn).
  - Dopo terminazione: stop polling (o rallenta) e UI in sola lettura.

B) Modifica bonus/malus non assegnati dopo l’avvio (solo creator)

- Una volta avviata la partita, il creatore ha il potere di modificare i bonus/malus CHE NON sono ancora stati assegnati a nessuno.
- Per le regole già assegnate:
  - Devono essere visibili, ma non modificabili.
- UI nel tab “Regole”:
  - Per le regole non assegnate: il creatore può aprire un Dialog “Modifica regola” con campi: Nome, Tipo (Bonus/Malus), Valore punti (ScoreDelta).
  - L’azione “Modifica” è di tipo **warning**.
  - Validazioni: nome obbligatorio, scoreDelta coerente (positivo per bonus, negativo per malus) oppure gestisci normalizzazione automatica e avvisa l’utente.

DTO / SHAPE API (da usare anche per mock)
Definisci DTO coerenti con i controller ipotizzati (senza esporre direttamente Entity, ma basati su quei campi):

1. POST /api/auth/login
   Req: { username: string; accessCode: string }
   Res: {
   token: string;
   game: Pick<GameEntity, "Id" | "Name" | "InitialScore" | "Status" | "CreatorPlayerId" | "WinnerPlayerId">;
   player: Pick<PlayerEntity, "Id" | "GameId" | "Username" | "IsCreator" | "CurrentScore">;
   }

2. POST /api/games/start (solo creator)
   Req: {
   name: string;
   initialScore: number;
   players: Array<{ username: string; accessCode: string; isCreator?: boolean }>;
   rules: Array<{ name: string; ruleType: 1|2; scoreDelta: number }>;
   }
   Res: {
   gameId: number;
   credentials: Array<{ username: string; accessCode: string; isCreator: boolean }>;
   }

3. GET /api/games/{gameId}/leaderboard
   Res: Array<Pick<PlayerEntity, "Id" | "Username" | "CurrentScore" | "IsCreator">>

4. GET /api/games/{gameId}/rules
   Res: Array<{
   rule: Pick<RuleEntity, "Id" | "Name" | "RuleType" | "ScoreDelta">;
   assignment: null | {
   ruleAssignmentId: number; // RuleAssignmentEntity.Id
   assignedToPlayerId: number;
   assignedToUsername: string; // derived via join
   assignedAt: string; // RuleAssignmentEntity.AssignedAt
   };
   }>

5. POST /api/games/{gameId}/rules/{ruleId}/assign
   Res: {
   assignment: {
   id: number;
   ruleId: number;
   assignedToPlayerId: number;
   assignedAt: string;
   scoreDeltaApplied: number;
   };
   updatedPlayer: Pick<PlayerEntity, "Id" | "CurrentScore">;
   gameStatus: { status: 2|3; winnerPlayerId?: number | null };
   }
   Error:

   - 409 Conflict se già assegnata (“La prima che”)

6. GET /api/games/{gameId}/status
   Res: {
   game: Pick<GameEntity, "Id" | "Status" | "WinnerPlayerId">;
   winner: null | Pick<PlayerEntity, "Id" | "Username" | "CurrentScore">;
   }

7. GET /api/games/{gameId}/assignments
   Res: Array<{
   id: number;
   ruleId: number;
   ruleName: string;
   assignedToPlayerId: number;
   assignedToUsername: string;
   scoreDeltaApplied: number;
   assignedAt: string;
   }>

NUOVI ENDPOINT (DA SUPPORTARE IN MOCK + PROVIDER) — OBBLIGATORI

8. POST /api/games/{gameId}/end (solo creator)
   Req: { } (vuoto)
   Res: {
   game: Pick<GameEntity, "Id" | "Status" | "WinnerPlayerId">;
   winner: Pick<PlayerEntity, "Id" | "Username" | "CurrentScore">;
   leaderboard: Array<Pick<PlayerEntity, "Id" | "Username" | "CurrentScore" | "IsCreator">>;
   }

9. PUT /api/games/{gameId}/rules/{ruleId} (solo creator, solo se non assegnata)
   Req: { name: string; ruleType: 1|2; scoreDelta: number }
   Res: {
   rule: Pick<RuleEntity, "Id" | "Name" | "RuleType" | "ScoreDelta">;
   }
   Error:
   - 409 Conflict se la regola è già assegnata (modifica non permessa)
   - 403 Forbidden se non creator

REQUISITI ARCHITETTURALI

1. HTTP CLIENT GENERICO (obbligatorio)

- Crea un HttpClient wrapper di fetch in src/lib/http/HttpClient.ts con:
  - baseUrl da env (VITE_API_BASE_URL)
  - JSON serialization/deserialization
  - supporto Authorization: Bearer <token> (token ottenuto da AuthProvider)
  - gestione errori centralizzata con typed errors (401, 403, 404, 409, 500)
  - metodi generici: get<T>(), post<TReq, TRes>(), put<...>(), delete<...>
  - un’unica implementazione, riusata da tutti i provider

2. PROVIDER PER OGNI CONTROLLER BACKEND (obbligatorio)
   Immagina i controller come segue e crea un provider per ciascuno:

- AuthProvider (AuthController)
- GamesProvider (GamesController) -> includi endGame()
- LeaderboardProvider (LeaderboardController) [può delegare a GamesProvider ma esponi comunque un hook dedicato]
- RulesProvider (RulesController) -> includi updateRule() (PUT)
- AssignmentsProvider (AssignmentsController)

Ogni provider deve:

- vivere in src/providers/<area>/
- esportare un hook custom useXxx()
- esporre metodi typed + stato loading/error
- non contenere componenti UI
- usare HttpClient (o mock adapter) senza duplicazioni

3. MOCK FACILMENTE RIMPIAZZABILI (obbligatorio)

- Frontend DEVE rispondere con MOCK (in-memory) ma sostituibili senza cambiare i componenti:
  - crea src/mocks/MockServer.ts + dataStore.ts + handlers.ts
  - implementa una “transport layer” selezionabile via env:
    - VITE_USE_MOCKS=true: provider usa MockTransport
    - VITE_USE_MOCKS=false: provider usa HttpClient reale
  - L’interfaccia del transport deve essere identica (stessi metodi e stesse shape DTO)
- Il mock deve implementare anche:
  - POST /end (calcola winner e chiude partita)
  - PUT /rules/{ruleId} con controllo “solo se non assegnata” e 409 in caso contrario

4. AUTENTICAZIONE / AUTORIZZAZIONE PLUGGABLE

- Implementa un pattern “AuthStrategy”:
  - src/lib/auth/AuthStrategy.ts (interfaccia)
  - MockAuthStrategy.ts
  - JwtAuthStrategy.ts (pronto ma non necessariamente collegato a server reale in questa fase)

Requisiti:

- Login con { username, accessCode } come da backend.
- Persistenza sessione in localStorage (token + player + gameId + role).
- Deve essere semplice cambiare strategia (env VITE_AUTH_STRATEGY=mock|jwt).
- Ruoli/permessi:
  - Role = "creator" | "player" determinato da PlayerEntity.IsCreator
- Implementa guard:
  - ProtectedRoute (richiede sessione)
  - RoleGuard (richiede ruolo)
- Centralizza mapping role/claims in un solo file.

5. SOLO SHADCN UI + THEME TOGGLE

- Usa esclusivamente componenti shadcn/ui:
  Button, Input, Card, Tabs, Table, Badge, Dialog, Separator, Label, Switch, DropdownMenu, Toaster, Skeleton, Alert, ecc.
- Implementa dark/light toggle nel layout header con persistenza (localStorage).
- Usa shadcn Toaster per notifiche (login error, 409 conflict, terminazione partita, modifica regola, ecc.) con background pieno.

6. HOOK CUSTOM E ZERO DUPLICAZIONE

- Crea hook riusabili:
  - useAsync: gestione promise/loading/error standard
  - usePolling: polling leaderboard/rules/status (intervallo da env VITE_POLLING_INTERVAL_MS)
  - useTheme: gestione tema
  - useAuth: accesso sessione e azioni
- Non duplicare mapping/DTO:
  - Crea tipi e DTO in un unico posto (src/types/)
  - Se servono view-model, crea mapper in src/lib/mappers/ (uno per area)

ROUTING (obbligatorio)

- "/" landing page
- "/game/:gameId" pagina partita

PAGINE / COMPONENTI (obbligatorio)

- LandingPage:
  - Colonna sinistra: LoginForm
  - Colonna destra: CreateGameForm
- GamePage:
  - Header con nome partita + stato + winner se ended
  - Aggiungi azione “Termina partita” visibile SOLO al creator quando Status=Started
  - Tabs:
    - LeaderboardTab
    - RulesTab (con modifica regole non assegnate se creator)

COMPORTAMENTI IMPORTANTI

- Dopo login, redirect a /game/:gameId
- Polling:
  - leaderboard + rules + status ogni N ms (env)
  - stop polling se Status=Ended (o rallenta)
- Gestione 401: logout + redirect landing
- Gestione 409 assign: toast + refresh rules/leaderboard
- Gestione 409 updateRule: toast “Questa regola è già stata assegnata e non può essere modificata” + refresh rules
- Le regole assegnate restano visibili ma non cliccabili (bottone disabled)
- Tutti i messaggi e label in italiano

ENVIRONMENT FILES (obbligatorio)
Imposta configurazione in:

- .env (default, safe)
- .env.local (override locale)

Variabili:

- VITE_API_BASE_URL=http://localhost:5001
- VITE_USE_MOCKS=true
- VITE_AUTH_STRATEGY=mock
- VITE_POLLING_INTERVAL_MS=3000
- VITE_APP_NAME=FantaSottone

STRUTTURA PROGETTO (obbligatoria)
src/
app/
App.tsx
router.tsx
providers/
AppProviders.tsx
components/
layout/
AppShell.tsx
Header.tsx
ThemeToggle.tsx
common/
LoadingState.tsx
ErrorState.tsx
EmptyState.tsx
ActionButton.tsx (NUOVO: variant info/success/warning/error + hover/focus)
toastStyles.ts (NUOVO: classi/varianti toaster con background pieno)
features/
auth/
pages/LandingPage.tsx
components/LoginForm.tsx
components/CreateGameForm.tsx
game/
pages/GamePage.tsx
components/LeaderboardTab.tsx
components/RulesTab.tsx
components/GameStatusBar.tsx
components/EndGameDialog.tsx (NUOVO)
components/EditRuleDialog.tsx (NUOVO)
lib/
http/
HttpClient.ts
Transport.ts
errors.ts
auth/
AuthStrategy.ts
MockAuthStrategy.ts
JwtAuthStrategy.ts
roles.ts
theme/
theme.ts
mappers/
...
providers/
auth/
AuthProvider.tsx
useAuth.ts
types.ts
games/
GamesProvider.tsx (include endGame)
useGames.ts
types.ts
leaderboard/
LeaderboardProvider.tsx
useLeaderboard.ts
types.ts
rules/
RulesProvider.tsx (include updateRule)
useRules.ts
types.ts
assignments/
AssignmentsProvider.tsx
useAssignments.ts
types.ts
mocks/
dataStore.ts
handlers.ts (include end + updateRule)
MockTransport.ts
MockServer.ts
types/
entities.ts
dto.ts
main.tsx
index.css

OUTPUT ATTESO

- Codice completo frontend (Vite + React TS) conforme ai vincoli sopra
- MockTransport con store in-memory che implementa:
  - avvio partita
  - login
  - assegnazione regole con 409
  - terminazione partita e calcolo vincitore
  - modifica regole non assegnate con 409 se già assegnata
- Provider e hook typed, senza duplicazioni
- .env e .env.local di esempio
- README con:
  - setup locale
  - come passare da mock a server reale (VITE_USE_MOCKS=false)
  - come cambiare strategia auth (VITE_AUTH_STRATEGY)
  - come estendere ruoli/permessi senza refactor
  - note UX: pattern colori info/success/warning/error e toaster con background pieno
