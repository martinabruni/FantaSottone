# Riepilogo Modifiche FantaSottone

## 🎯 Obiettivi
1. ✅ Creare prima il creator, poi la partita
2. ✅ Permettere al creator di creare nuove regole durante la partita
3. ✅ Permettere al creator di eliminare/modificare regole non assegnate

## 📋 Modifiche Principali

### Backend

| File | Modifica | Tipo |
|------|----------|------|
| `GameManager.cs` | Nuova sequenza creazione (creator → game → players → rules) | MODIFICATO |
| `IRuleService.cs` | Aggiunti `CreateRuleAsync` e `DeleteRuleAsync` | MODIFICATO |
| `RuleService.cs` | Implementazione create/delete con controlli autorizzazione | MODIFICATO |
| `GamesController.cs` | Endpoint `POST /{gameId}/rules` e `DELETE /{gameId}/rules/{ruleId}` | MODIFICATO |
| `ApiDtos.cs` | DTO `CreateRuleRequest/Response` | MODIFICATO |

### Frontend

| File | Modifica | Tipo |
|------|----------|------|
| `dto.ts` | Tipi TypeScript per create/delete | MODIFICATO |
| `RulesProvider.tsx` | Metodi `createRule` e `deleteRule` | MODIFICATO |
| `CreateRuleDialog.tsx` | Dialog per creare nuove regole | **NUOVO** |
| `RulesTab.tsx` | UI per create/delete con bottoni e gestione errori | MODIFICATO |
| `handlers.ts` | Mock handler create/delete + nuova sequenza startGame | MODIFICATO |
| `dataStore.ts` | Metodo `deleteRule` | MODIFICATO |
| `MockTransport.ts` | Routing POST/DELETE per regole | MODIFICATO |

## 🔐 Permessi

| Azione | Creator | Player |
|--------|---------|--------|
| Creare regola | ✅ | ❌ |
| Modificare regola non assegnata | ✅ | ❌ |
| Eliminare regola non assegnata | ✅ | ❌ |
| Modificare regola assegnata | ❌ | ❌ |
| Eliminare regola assegnata | ❌ | ❌ |
| Assegnare regola | ✅ | ✅ |

## 🔄 Nuova Sequenza Creazione

```
VECCHIA:                      NUOVA:
1. Crea Game (Draft)    →     1. Crea Creator Player
2. Crea Players         →     2. Crea Game (Started, con CreatorPlayerId)
3. Aggiorna Game        →     3. Aggiorna Creator con GameId
4. Status → Started     →     4. Crea altri Players
5. Crea Rules          →     5. Crea Rules
```

## 🎨 UI Changes

### RulesTab (solo creator)
- ➕ Bottone "Crea nuova regola" in alto a destra
- 🗑️ Bottone "Elimina" per regole non assegnate
- ✏️ Bottone "Modifica" per regole non assegnate (già esistente)

### CreateRuleDialog (nuovo)
- Campo Nome
- Selector Tipo (Bonus/Malus)
- Campo Punti (auto-normalizzato in base al tipo)

## 📡 Nuovi Endpoint

| Metodo | Path | Descrizione | Auth |
|--------|------|-------------|------|
| POST | `/api/games/{gameId}/rules` | Crea nuova regola | ✅ Creator |
| DELETE | `/api/games/{gameId}/rules/{ruleId}` | Elimina regola non assegnata | ✅ Creator |

## ⚠️ Breaking Changes

- ❌ La sequenza di creazione partita è cambiata
- ✅ Nessuna modifica allo schema DB
- ✅ Endpoint esistenti compatibili
- ✅ DTO format compatibile

## ✅ Testing

### Checklist Veloce
```
[ ] Creare partita → verificare creator creato per primo
[ ] Login creator → verificare badge "Creatore"
[ ] Creare regola → verificare apparizione in lista
[ ] Eliminare regola non assegnata → verificare scomparsa
[ ] Assegnare regola → verificare bottone elimina sparisce
[ ] Tentare eliminare regola assegnata → verificare errore 409
[ ] Login player → verificare bottoni create/delete non visibili
```

## 🚀 Deploy

1. Backend: Sostituire 5 file
2. Frontend: Sostituire 6 file + creare 1 nuovo file
3. Nessuna migrazione DB necessaria
4. Test end-to-end prima del deploy in produzione

## 📝 Note

- Il polling mostra le modifiche agli altri giocatori ogni 3-5s
- Errore 409 se si tenta di modificare/eliminare regola assegnata
- Errore 403 se non-creator tenta operazioni riservate
- La partita ora parte **direttamente in Started** (salta Draft)

## 📚 Documentazione Completa

- `README_MODIFICHE.md` - Dettagli tecnici completi
- `GUIDA_APPLICAZIONE.md` - Istruzioni passo-passo per deploy
