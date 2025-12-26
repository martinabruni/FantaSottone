# FantaSottone - Refactoring Architetturale

## Modifiche Implementate

### 1. Repository Layer

#### IRepository<TEntity, TKey>
- **RIMOSSO**: `SaveChangesAsync()` - ogni operazione ora salva automaticamente
- **MODIFICATO**: Tutti i metodi ritornano `AppResult<T>` invece di `T` o `T?`
- **BENEFICI**: 
  - Gestione errori consistente
  - Eliminazione di chiamate SaveChanges dimenticate
  - Migliore tracciabilità degli errori

#### BaseRepository<TDomainEntity, TDbEntity, TKey>
- **AGGIUNTO**: Integrazione Mapster per mapping automatico
- **AGGIUNTO**: Try-catch con logging strutturato
- **AGGIUNTO**: Gestione specializzata degli errori:
  - DbUpdateException (unique constraints) → 409 Conflict
  - DbUpdateConcurrencyException → 409 Conflict
  - Foreign key violations → 409 Conflict
  - Not found → 404 Not Found
  - Generic errors → 500 Internal Server Error

#### Repository Specifici
- **GameRepository**: `GetByIdWithDetailsAsync()` con eager loading
- **PlayerRepository**: 
  - `GetByCredentialsAsync()` ritorna 401 se credenziali invalide
  - `GetLeaderboardAsync()` con ordinamento score DESC, ID ASC (tie-break)
  - `CountPlayersWithScoreLessThanOrEqualToZeroAsync()` per end-game condition
- **RuleRepository**: Query filtrate per gameId
- **RuleAssignmentRepository**: 
  - **CRITICO**: Override di `AddAsync()` per gestire "La prima che" con 409 su unique constraint
  - Gestione atomica race conditions su RuleId

### 2. Service Layer (Da Aggiornare)

#### Modifiche Necessarie
- **RIMUOVERE**: Chiamate a `SaveChangesAsync()`
- **AGGIORNARE**: Gestione dei risultati da `AppResult` dei repository
- **AGGIUNGERE**: Try-catch per logica business complessa
- **MANTENERE**: Orchestrazione multi-repository in transazioni quando necessario

#### Pattern Suggerito
```csharp
public async Task<AppResult<TEntity>> SomeServiceMethod(...)
{
    try
    {
        // Get da repository
        var entityResult = await _repository.GetByIdAsync(id, cancellationToken);
        if (entityResult.IsFailure)
            return entityResult; // Propaga errore

        var entity = entityResult.Value!;
        
        // Business logic
        entity.SomeProperty = newValue;
        
        // Update (auto-save incluso)
        return await _repository.UpdateAsync(entity, cancellationToken);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in SomeServiceMethod");
        return AppResult<TEntity>.InternalServerError($"Service error: {ex.Message}");
    }
}
```

### 3. Transazioni Multi-Repository

Per operazioni che coinvolgono più repository (es. StartGame, AssignRule):

```csharp
using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
try
{
    var gameResult = await _gameRepository.AddAsync(game, cancellationToken);
    if (gameResult.IsFailure)
    {
        await transaction.RollbackAsync(cancellationToken);
        return AppResult<StartGameResult>.BadRequest(gameResult.Errors);
    }
    
    // ... altre operazioni
    
    await transaction.CommitAsync(cancellationToken);
    return AppResult<StartGameResult>.Created(result);
}
catch (Exception ex)
{
    await transaction.RollbackAsync(cancellationToken);
    _logger.LogError(ex, "Transaction failed");
    return AppResult<StartGameResult>.InternalServerError($"Transaction error: {ex.Message}");
}
```

### 4. Gestione "La Prima Che"

Il meccanismo atomico è implementato in `RuleAssignmentRepository.AddAsync()`:
- Unique constraint `UX_RuleAssignmentEntity_RuleId` sul database
- Catch `DbUpdateException` con check sul constraint name
- Ritorno `409 Conflict` con code `RULE_ALREADY_ASSIGNED`

Il service `RuleAssignmentService.AssignRuleAsync()` deve:
1. Verificare rule/player/game esistano
2. Chiamare `AddAsync()` sul repository
3. Se 409, propagare al controller
4. Aggiornare player score in stessa transazione se necessario

### 5. Dependencies

Aggiungere al progetto Infrastructure:
```xml
<PackageReference Include="Mapster" Version="7.4.0" />
<PackageReference Include="Mapster.DependencyInjection" Version="1.0.1" />
```

### 6. ServiceCollection Extensions

```csharp
services.AddScoped(typeof(IRepository<,>), typeof(BaseRepository<,,>));
services.AddMapster(); // Auto-registra configurazioni Mapster
```

## Prossimi Passi

1. ✅ Repository Layer completato
2. ⏳ Aggiornare Service Layer
3. ⏳ Aggiornare Manager Layer  
4. ⏳ Verificare Controller (già allineati se usano AppResult)
5. ⏳ Testing: verificare race conditions su assign
6. ⏳ Integration tests per transazioni multi-repo
