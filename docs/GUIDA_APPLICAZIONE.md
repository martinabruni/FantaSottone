# Guida Rapida - Applicazione Modifiche

## File da Sostituire/Aggiornare

### Backend (.NET)

#### Da Sostituire Completamente:
1. `src/Businesses/Internal.FantaSottone.Business/Managers/GameManager.cs`
2. `src/Businesses/Internal.FantaSottone.Business/Services/RuleService.cs`
3. `src/Domains/Internal.FantaSottone.Domain/Services/IRuleService.cs`
4. `src/Apis/Internal.FantaSottone.Api/Controllers/GamesController.cs`
5. `src/Apis/Internal.FantaSottone.Api/DTOs/ApiDtos.cs`

### Frontend (React/TypeScript)

#### Da Sostituire Completamente:
1. `src/types/dto.ts`
2. `src/providers/rules/RulesProvider.tsx`
3. `src/components/features/game/components/RulesTab.tsx`
4. `src/mocks/handlers.ts`
5. `src/mocks/dataStore.ts`
6. `src/mocks/MockTransport.ts`

#### Da Creare (Nuovi File):
1. `src/components/features/game/components/CreateRuleDialog.tsx`

## Checklist Pre-Deploy

### Backend
- [ ] Sostituire tutti i file backend elencati sopra
- [ ] Verificare che il progetto compili senza errori
- [ ] Testare endpoint con Swagger:
  - [ ] POST /api/games/start (verificare nuova sequenza)
  - [ ] POST /api/games/{gameId}/rules (creare regola)
  - [ ] DELETE /api/games/{gameId}/rules/{ruleId} (eliminare regola)

### Frontend
- [ ] Sostituire tutti i file frontend elencati sopra
- [ ] Creare il nuovo file CreateRuleDialog.tsx
- [ ] Verificare che il progetto compili senza errori (`npm run build`)
- [ ] Testare in modalità mock (`VITE_USE_MOCKS=true`):
  - [ ] Creare una nuova partita
  - [ ] Login come creator
  - [ ] Creare nuova regola durante la partita
  - [ ] Eliminare regola non assegnata
  - [ ] Tentare di eliminare regola assegnata (deve dare errore)

## Test End-to-End (con Backend Reale)

1. **Test Creazione Partita:**
   ```
   - Andare alla landing page
   - Compilare form "Crea nuova partita"
   - Aggiungere 3 giocatori (primo è creator)
   - Aggiungere 2 regole
   - Premere "Crea partita"
   - Verificare che venga mostrata la lista credenziali
   - Copiare credenziali del creator
   ```

2. **Test Login Creator:**
   ```
   - Tornare alla landing page
   - Usare credenziali del creator
   - Premere "Accedi"
   - Verificare redirect a /game/:gameId
   - Verificare che venga mostrato badge "Creatore" nell'header
   ```

3. **Test Crea Regola:**
   ```
   - Andare alla tab "Regole"
   - Verificare presenza bottone "Crea nuova regola"
   - Premere il bottone
   - Compilare form:
     * Nome: "Test Bonus"
     * Tipo: Bonus
     * Punti: 15
   - Premere "Crea regola"
   - Verificare che la regola appaia nella lista
   - Verificare toast di successo
   ```

4. **Test Elimina Regola Non Assegnata:**
   ```
   - Nella tab "Regole"
   - Per la regola appena creata, verificare presenza bottone "Elimina"
   - Premere "Elimina"
   - Verificare che la regola sparisca
   - Verificare toast di successo
   ```

5. **Test Modifica Regola Non Assegnata:**
   ```
   - Creare una nuova regola
   - Premere bottone "Modifica"
   - Cambiare il nome
   - Premere "Salva modifiche"
   - Verificare che il nome sia cambiato
   ```

6. **Test Assegna e Tentativo Eliminazione:**
   ```
   - Assegnare una regola a sé stessi
   - Tentare di eliminare la regola appena assegnata
   - Verificare errore 409
   - Verificare toast "Questa regola è già stata assegnata..."
   - Verificare che il bottone "Elimina" non sia più visibile
   ```

7. **Test Permessi Non-Creator:**
   ```
   - Logout
   - Login come giocatore non-creator
   - Andare alla tab "Regole"
   - Verificare che NON ci sia il bottone "Crea nuova regola"
   - Verificare che NON ci siano bottoni "Elimina" e "Modifica"
   ```

## Rollback (in caso di problemi)

Se qualcosa va storto:

### Backend
1. Ripristinare i file originali da Git
2. Ricompilare
3. Riavviare il server

### Frontend
1. Ripristinare i file originali da Git
2. Ricompilare (`npm run build`)
3. Deployare la versione precedente

## Note Importanti

⚠️ **BREAKING CHANGE:**
La sequenza di creazione partita è cambiata. Se hai partite create con la vecchia versione che sono ancora in stato "Draft", potrebbero non funzionare correttamente.

✅ **COMPATIBILITÀ:**
- Le partite già create e in stato "Started" o "Ended" funzioneranno normalmente
- Gli endpoint esistenti (assegnazione, leaderboard, status) non sono cambiati
- La struttura del database non è cambiata

## Supporto

In caso di problemi, verificare:
1. I log del backend per errori di validazione
2. La console browser per errori JavaScript
3. Le chiamate API nel Network tab del browser
4. I toast di errore nell'applicazione

Per ulteriori dettagli, consultare:
- `README_MODIFICHE.md` - Documentazione completa delle modifiche
- Swagger UI - Per testare manualmente gli endpoint
- DevTools Console - Per debug frontend
