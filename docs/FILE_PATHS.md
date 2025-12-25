# Lista Completa File Modificati

## Backend (.NET C#)

### File da Sostituire Completamente

```
src/Businesses/Internal.FantaSottone.Business/Managers/GameManager.cs
src/Businesses/Internal.FantaSottone.Business/Services/RuleService.cs
src/Domains/Internal.FantaSottone.Domain/Services/IRuleService.cs
src/Apis/Internal.FantaSottone.Api/Controllers/GamesController.cs
src/Apis/Internal.FantaSottone.Api/DTOs/ApiDtos.cs
```

## Frontend (React TypeScript)

### File da Sostituire Completamente

```
src/Apis/Internal.FantaSottone.React/src/types/dto.ts
src/Apis/Internal.FantaSottone.React/src/providers/rules/RulesProvider.tsx
src/Apis/Internal.FantaSottone.React/src/components/features/game/components/RulesTab.tsx
src/Apis/Internal.FantaSottone.React/src/mocks/handlers.ts
src/Apis/Internal.FantaSottone.React/src/mocks/dataStore.ts
src/Apis/Internal.FantaSottone.React/src/mocks/MockTransport.ts
```

### File da Creare (Nuovo)

```
src/Apis/Internal.FantaSottone.React/src/components/features/game/components/CreateRuleDialog.tsx
```

## Comandi Git per Applicare Modifiche

### Backend

```bash
# Nella root del repository backend

# Copia i nuovi file (assumendo che siano in /tmp/fantasy_updates/)
cp /tmp/fantasy_updates/GameManager.cs src/Businesses/Internal.FantaSottone.Business/Managers/
cp /tmp/fantasy_updates/RuleService.cs src/Businesses/Internal.FantaSottone.Business/Services/
cp /tmp/fantasy_updates/IRuleService.cs src/Domains/Internal.FantaSottone.Domain/Services/
cp /tmp/fantasy_updates/GamesController.cs src/Apis/Internal.FantaSottone.Api/Controllers/
cp /tmp/fantasy_updates/ApiDtos.cs src/Apis/Internal.FantaSottone.Api/DTOs/

# Verifica modifiche
git status

# Commit
git add .
git commit -m "feat: modificata meccanica creazione partita e gestione regole

- Creator viene creato prima della partita
- Creator può creare nuove regole durante la partita
- Creator può eliminare/modificare regole non assegnate
- Aggiunti endpoint POST/DELETE per regole
"
```

### Frontend

```bash
# Nella root del repository frontend

# Copia i file modificati
cp /tmp/fantasy_updates/dto.ts src/types/
cp /tmp/fantasy_updates/RulesProvider.tsx src/providers/rules/
cp /tmp/fantasy_updates/RulesTab.tsx src/components/features/game/components/
cp /tmp/fantasy_updates/handlers.ts src/mocks/
cp /tmp/fantasy_updates/dataStore.ts src/mocks/
cp /tmp/fantasy_updates/MockTransport.ts src/mocks/

# Crea il nuovo file
cp /tmp/fantasy_updates/CreateRuleDialog.tsx src/components/features/game/components/

# Verifica modifiche
git status

# Build per verificare che non ci siano errori
npm run build

# Commit
git add .
git commit -m "feat: aggiunta gestione creazione/eliminazione regole

- Aggiunto CreateRuleDialog per creare nuove regole
- Modificato RulesTab per includere bottoni create/delete
- Aggiornati mock handlers per supportare nuove API
- Creator può gestire regole durante la partita
"
```

## File di Documentazione

Questi file possono essere aggiunti al repository per riferimento:

```
docs/RIEPILOGO_MODIFICHE.md          (questo file riassuntivo)
docs/README_MODIFICHE.md              (documentazione completa)
docs/GUIDA_APPLICAZIONE.md            (guida deploy step-by-step)
```

## Verifica Post-Modifica

### Backend
```bash
# Build
dotnet build

# Run
dotnet run --project src/Apis/Internal.FantaSottone.Api

# Test con curl
curl -X POST http://localhost:5214/api/games/start \
  -H "Content-Type: application/json" \
  -d @test_game.json
```

### Frontend
```bash
# Install dependencies (se necessario)
npm install

# Build
npm run build

# Run dev
npm run dev

# Aprire http://localhost:5173
```

## Dimensione Modifiche

| Categoria | Linee Aggiunte | Linee Rimosse | File Modificati | File Nuovi |
|-----------|----------------|---------------|-----------------|------------|
| Backend   | ~150           | ~50           | 5               | 0          |
| Frontend  | ~300           | ~100          | 6               | 1          |
| **Totale** | **~450**      | **~150**      | **11**          | **1**      |

## Impact Assessment

| Area | Impatto | Note |
|------|---------|------|
| Database | ✅ Nessuno | Nessuna modifica schema |
| API Contracts | ⚠️ Minore | Solo aggiunti nuovi endpoint |
| Business Logic | ⚠️ Medio | Cambiata sequenza creazione partita |
| UI/UX | ⚠️ Medio | Nuovi controlli per creator |
| Testing | ⚠️ Alto | Richiesti nuovi test end-to-end |
| Deployment | ✅ Basso | Nessuna migrazione richiesta |

## Compatibilità Versioni

| Componente | Versione Min | Versione Testata |
|------------|--------------|------------------|
| .NET | 8.0 | 8.0 |
| React | 18+ | 18.3.1 |
| TypeScript | 5+ | 5.5.3 |
| EF Core | 8.0 | 8.0.20 |

## Link Utili

- Swagger UI: `http://localhost:5214/swagger`
- Frontend Dev: `http://localhost:5173`
- React DevTools: Ctrl+Shift+I → Components
- Network Monitor: Ctrl+Shift+I → Network
