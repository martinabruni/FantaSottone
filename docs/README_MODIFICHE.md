# Modifiche alla Meccanica FantaSottone

Questo documento descrive le modifiche implementate per cambiare la meccanica di creazione partita e gestione regole.

## Modifiche Implementate

### 1. Nuova Sequenza di Creazione Partita

**Prima:**
- La partita veniva creata in stato Draft
- Poi venivano creati i giocatori
- Infine lo stato passava a Started

**Dopo:**
- Il **creator viene creato per primo** (con GameId temporaneo)
- Poi viene creata la **partita con CreatorPlayerId già impostato**
- Il creator viene aggiornato con il GameId corretto
- Vengono creati gli altri giocatori
- Infine vengono create le regole
- La partita parte **direttamente in stato Started** (non passa più da Draft)

**File Modificati:**
- `GameManager.cs` - Modificata la logica in `StartGameAsync`
- `handlers.ts` (mock) - Aggiornato il mock per riflettere la nuova sequenza

### 2. Gestione Regole Durante la Partita

Il creator può ora:

#### a) Creare nuove regole durante la partita
- Nuovo endpoint: `POST /api/games/{gameId}/rules`
- Solo il creator può creare nuove regole
- Le regole possono essere create anche con partita in corso

#### b) Modificare regole non assegnate
- Endpoint esistente: `PUT /api/games/{gameId}/rules/{ruleId}`
- Solo il creator può modificare
- Solo se la regola **non è stata ancora assegnata**

#### c) Eliminare regole non assegnate
- Nuovo endpoint: `DELETE /api/games/{gameId}/rules/{ruleId}`
- Solo il creator può eliminare
- Solo se la regola **non è stata ancora assegnata**

## File Backend Modificati

### Nuovi/Aggiornati
1. **`GameManager.cs`**
   - Modificata sequenza creazione partita
   - Creator creato per primo, partita parte direttamente Started

2. **`IRuleService.cs`**
   - Aggiunti metodi `CreateRuleAsync` e `DeleteRuleAsync`

3. **`RuleService.cs`**
   - Implementati `CreateRuleAsync` e `DeleteRuleAsync`
   - Controlli di autorizzazione (solo creator)
   - Controlli sullo stato assegnazione regola

4. **`GamesController.cs`**
   - Aggiunto endpoint `POST /{gameId}/rules` (create)
   - Aggiunto endpoint `DELETE /{gameId}/rules/{ruleId}` (delete)

5. **`ApiDtos.cs`**
   - Aggiunti `CreateRuleRequest` e `CreateRuleResponse`

## File Frontend Modificati

### Nuovi/Aggiornati
1. **`dto.ts`**
   - Aggiunti tipi per `CreateRuleRequest` e `CreateRuleResponse`

2. **`RulesProvider.tsx`**
   - Aggiunti metodi `createRule` e `deleteRule`

3. **`CreateRuleDialog.tsx`** (nuovo)
   - Dialog per creare nuove regole
   - Validazione input
   - Normalizzazione scoreDelta basata su RuleType

4. **`RulesTab.tsx`**
   - Aggiunto bottone "Crea nuova regola" (solo per creator)
   - Aggiunto bottone "Elimina" per regole non assegnate
   - Gestione stati e errori per create/delete

5. **`handlers.ts`** (mock)
   - Implementato handler `createRule`
   - Implementato handler `deleteRule`
   - Aggiornato handler `startGame` per nuova sequenza

6. **`dataStore.ts`** (mock)
   - Aggiunto metodo `deleteRule`

7. **`MockTransport.ts`**
   - Aggiunto routing per `POST /api/games/{gameId}/rules`
   - Aggiunto routing per `DELETE /api/games/{gameId}/rules/{ruleId}`

## Regole di Business

### Permessi Creator
Il creator può:
- ✅ Creare nuove regole in qualsiasi momento
- ✅ Modificare regole non ancora assegnate
- ✅ Eliminare regole non ancora assegnate
- ❌ Modificare regole già assegnate
- ❌ Eliminare regole già assegnate

### Validazioni
- Una regola può essere modificata/eliminata solo se **non è stata assegnata**
- Solo il **creator della partita** può creare/modificare/eliminare regole
- Il backend risponde con `409 Conflict` se si tenta di modificare/eliminare una regola assegnata
- Il backend risponde con `403 Forbidden` se un non-creator tenta di creare/modificare/eliminare

## UI/UX

### RulesTab - Novità
1. **Bottone "Crea nuova regola"** (visibile solo al creator)
   - Apre un dialog per inserire nome, tipo e punteggio
   - Il punteggio viene automaticamente normalizzato (positivo per Bonus, negativo per Malus)

2. **Bottone "Elimina"** per ogni regola (visibile solo al creator)
   - Visibile solo per regole non ancora assegnate
   - Richiede conferma
   - Mostra errore se la regola è già stata assegnata

3. **Bottone "Modifica"** per ogni regola (già esistente)
   - Visibile solo al creator
   - Visibile solo per regole non ancora assegnate

### Feedback Utente
- Toast di successo quando una regola viene creata
- Toast di successo quando una regola viene eliminata
- Toast di errore 409 se si tenta di eliminare/modificare una regola assegnata
- Toast di errore 403 se un non-creator tenta operazioni riservate

## Test

### Test Scenario 1: Creazione Partita
1. Aprire il form "Crea nuova partita"
2. Inserire nome e punteggio iniziale
3. Aggiungere almeno 2 giocatori (il primo è sempre creator)
4. Aggiungere almeno 1 regola
5. Premere "Crea partita"

**Risultato atteso:**
- Il creator viene creato per primo
- La partita parte direttamente in stato "Started"
- Tutti i giocatori sono collegati alla partita

### Test Scenario 2: Crea Regola Durante Partita
1. Login come creator
2. Navigare alla tab "Regole"
3. Premere "Crea nuova regola"
4. Compilare nome, tipo e punteggio
5. Premere "Crea regola"

**Risultato atteso:**
- La nuova regola appare nella lista
- È immediatamente assegnabile
- Gli altri giocatori la vedono dopo il prossimo polling

### Test Scenario 3: Elimina Regola Non Assegnata
1. Login come creator
2. Navigare alla tab "Regole"
3. Per una regola non assegnata, premere il bottone "Elimina"

**Risultato atteso:**
- La regola viene rimossa
- Tutti i giocatori smettono di vederla dopo il polling

### Test Scenario 4: Tentativo Elimina Regola Assegnata
1. Un giocatore assegna una regola a sé stesso
2. Il creator tenta di eliminare quella regola

**Risultato atteso:**
- Errore 409 Conflict
- Toast con messaggio "Questa regola è già stata assegnata e non può essere eliminata"
- La regola rimane nella lista

## Compatibilità

### Retrocompatibilità
- ❌ **Breaking change:** La sequenza di creazione partita è cambiata
- ✅ Gli endpoint esistenti continuano a funzionare
- ✅ Il formato dei DTO non è cambiato (solo aggiunti nuovi DTO)

### Database
- ✅ Nessuna modifica allo schema del database necessaria
- ✅ Il campo `GameEntity.Status` supporta già Draft/Started/Ended
- ✅ Il campo `GameEntity.CreatorPlayerId` viene ora impostato correttamente durante la creazione

## Note per il Deploy

1. **Backend:** Deployare la nuova versione del backend
2. **Frontend:** Deployare la nuova versione del frontend
3. **Nessuna migrazione DB richiesta**

## Limitazioni Note

1. **Polling:** Le modifiche alle regole sono visibili agli altri giocatori solo dopo il prossimo ciclo di polling (3-5 secondi)
2. **Conflict Resolution:** Se due creator provano a creare regole contemporaneamente su partite diverse, non ci sono conflitti (le partite sono isolate)

## Future Enhancements

Possibili miglioramenti futuri:
- Aggiungere WebSocket per aggiornamenti real-time invece di polling
- Aggiungere drag & drop per riordinare le regole
- Aggiungere preview delle modifiche prima di salvarle
- Aggiungere undo/redo per modifiche regole
- Aggiungere bulk operations (crea/elimina multiple regole)
