# FantaSottone - Guida Completa al Refactoring

## 📋 Sommario delle Modifiche

### 1. Architettura Repository

#### Prima (Problemi)
```csharp
public interface IRepository<TEntity, TKey>
{
    Task<TEntity?> GetByIdAsync(TKey id, ...);
    Task<TEntity> AddAsync(TEntity entity, ...);
    Task<int> SaveChangesAsync(...); // ❌ Chiamata dimenticata
}

// Uso
var entity = await _repository.AddAsync(newEntity, ct);
await _repository.SaveChangesAsync(ct); // ❌ Spesso dimenticato!
```

#### Dopo (Soluzioni)
```csharp
public interface IRepository<TEntity, TKey>
{
    Task<AppResult<TEntity>> GetByIdAsync(TKey id, ...);
    Task<AppResult<TEntity>> AddAsync(TEntity entity, ...); // ✅ Auto-save
    // ✅ SaveChanges rimosso
}

// Uso
var result = await _repository.AddAsync(newEntity, ct); // ✅ Salva automaticamente
if (result.IsFailure) 
    return result; // ✅ Gestione errori consistente
```

### 2. BaseRepository con Mapster

#### Benefici
- ✅ Mapping automatico tra Domain Models e DB Entities
- ✅ Try-catch centralizzato con logging
- ✅ Gestione specializzata degli errori (409, 404, 500)
- ✅ Auto-save dopo ogni operazione

#### Esempio Implementazione
```csharp
internal abstract class BaseRepository<TDomainEntity, TDbEntity, TKey>
{
    protected readonly FantaSottoneContext _context;
    
    public virtual async Task<AppResult<TDomainEntity>> AddAsync(...)
    {
        try
        {
            var dbEntity = entity.Adapt<TDbEntity>(); // Mapster
            await _dbSet.AddAsync(dbEntity, ct);
            await _context.SaveChangesAsync(ct); // ✅ Auto-save
            return AppResult<TDomainEntity>.Created(dbEntity.Adapt<TDomainEntity>());
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE"))
        {
            return AppResult<TDomainEntity>.Conflict("Duplicate", "DUPLICATE");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating entity");
            return AppResult<TDomainEntity>.InternalServerError($"Error: {ex.Message}");
        }
    }
}
```

### 3. Gestione "La Prima Che"

#### Meccanismo Atomico
```csharp
// Database: UNIQUE INDEX UX_RuleAssignmentEntity_RuleId

// RuleAssignmentRepository.AddAsync() override
public override async Task<AppResult<RuleAssignment>> AddAsync(...)
{
    try
    {
        await _dbSet.AddAsync(dbEntity, ct);
        await _context.SaveChangesAsync(ct);
        return AppResult<RuleAssignment>.Created(...);
    }
    catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UX_RuleAssignmentEntity_RuleId"))
    {
        // ✅ Race condition rilevata
        return AppResult<RuleAssignment>.Conflict(
            "Rule already assigned by another player", 
            "RULE_ALREADY_ASSIGNED");
    }
}
```

#### Service con Transazione
```csharp
public async Task<AppResult<RuleAssignment>> AssignRuleAsync(...)
{
    using var transaction = await _context.Database.BeginTransactionAsync(ct);
    try
    {
        // 1. Verifica game/rule/player
        // 2. Aggiorna score player
        var updateResult = await _playerRepository.UpdateAsync(player, ct);
        if (updateResult.IsFailure) { rollback; return error; }
        
        // 3. Crea assignment (atomico via unique constraint)
        var assignResult = await _ruleAssignmentRepository.AddAsync(assignment, ct);
        if (assignResult.IsFailure) { rollback; return error; } // Propaga 409
        
        await transaction.CommitAsync(ct);
        return assignResult;
    }
    catch (Exception ex) { rollback; return error; }
}
```

### 4. Service Layer Pattern

#### Template Standard
```csharp
public async Task<AppResult<TEntity>> SomeMethod(...)
{
    try
    {
        // 1. Get from repository
        var result = await _repository.GetByIdAsync(id, ct);
        if (result.IsFailure)
            return result; // ✅ Propaga errore
        
        var entity = result.Value!;
        
        // 2. Business logic
        entity.Property = newValue;
        
        // 3. Update (auto-save)
        return await _repository.UpdateAsync(entity, ct);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in SomeMethod");
        return AppResult<TEntity>.InternalServerError($"Error: {ex.Message}");
    }
}
```

#### Multi-Repository con Transazione
```csharp
public async Task<AppResult<TResult>> ComplexOperation(...)
{
    using var transaction = await _context.Database.BeginTransactionAsync(ct);
    try
    {
        var result1 = await _repo1.AddAsync(entity1, ct);
        if (result1.IsFailure) { await transaction.RollbackAsync(ct); return ...; }
        
        var result2 = await _repo2.UpdateAsync(entity2, ct);
        if (result2.IsFailure) { await transaction.RollbackAsync(ct); return ...; }
        
        await transaction.CommitAsync(ct);
        return AppResult<TResult>.Success(...);
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync(ct);
        return AppResult<TResult>.InternalServerError($"Transaction error: {ex.Message}");
    }
}
```

## 🔧 Modifiche ai File di Progetto

### Infrastructure.csproj
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.11" />
  <PackageReference Include="Mapster" Version="7.4.0" />
  <PackageReference Include="Mapster.DependencyInjection" Version="1.0.1" />
</ItemGroup>
```

### ServiceCollectionExtensions (Infrastructure)
```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        string connectionString)
    {
        // DbContext
        services.AddDbContext<FantaSottoneContext>(options =>
            options.UseSqlServer(connectionString));

        // Mapster
        services.AddMapster();

        // Repositories
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IRuleRepository, RuleRepository>();
        services.AddScoped<IRuleAssignmentRepository, RuleAssignmentRepository>();

        return services;
    }
}
```

## 🎯 Verifiche Controller

I controller esistenti dovrebbero già essere compatibili se usano `AppResult`. Verifica:

### ✅ Pattern Corretto
```csharp
[HttpPost("assign")]
public async Task<IActionResult> AssignRule(...)
{
    var result = await _service.AssignRuleAsync(..., ct);
    
    if (result.IsFailure)
    {
        return StatusCode((int)result.StatusCode, new ProblemDetails
        {
            Status = (int)result.StatusCode,
            Title = result.Errors.FirstOrDefault()?.Message,
            Type = result.Errors.FirstOrDefault()?.Code  // ✅ "RULE_ALREADY_ASSIGNED" per frontend
        });
    }
    
    return Ok(MapToDto(result.Value!));
}
```

### ❌ Anti-Pattern
```csharp
// ❌ Non verificare IsFailure
var result = await _service.GetByIdAsync(id, ct);
var entity = result.Value; // ❌ Null se failure!

// ❌ Non usare try-catch nel controller
try { var result = await _service.DoSomething(); }
catch (Exception ex) { return StatusCode(500); } // Service gestisce già
```

## 📊 Gestione Errori Standardizzata

### Codici HTTP e AppStatusCode
| AppStatusCode | HTTP | Scenario |
|---------------|------|----------|
| Ok | 200 | Successo |
| Created | 201 | Entità creata |
| BadRequest | 400 | Validazione fallita |
| Unauthorized | 401 | Credenziali invalide |
| Forbidden | 403 | Non autorizzato (es. non creator) |
| NotFound | 404 | Entità non trovata |
| **Conflict** | **409** | **Duplicate / Race condition / "La prima che"** |
| InternalServerError | 500 | Errore generico |

### Error Codes per Frontend
```csharp
// Repository ritorna
AppResult<RuleAssignment>.Conflict(
    "Rule already assigned", 
    "RULE_ALREADY_ASSIGNED"); // ✅ Codice specifico

// Frontend riceve
{
  "status": 409,
  "title": "Rule already assigned",
  "type": "RULE_ALREADY_ASSIGNED" // ✅ Frontend può fare switch(type)
}
```

## 🧪 Testing Raccomandato

### 1. Unit Test Repository
```csharp
[Fact]
public async Task AddAsync_WhenUniqueConstraintViolated_ReturnsConflict()
{
    // Arrange
    var assignment1 = new RuleAssignment { RuleId = 1, ... };
    await _repository.AddAsync(assignment1, ct);
    
    var assignment2 = new RuleAssignment { RuleId = 1, ... }; // Stesso RuleId
    
    // Act
    var result = await _repository.AddAsync(assignment2, ct);
    
    // Assert
    Assert.True(result.IsFailure);
    Assert.Equal(AppStatusCode.Conflict, result.StatusCode);
    Assert.Contains("RULE_ALREADY_ASSIGNED", result.Errors.First().Code);
}
```

### 2. Integration Test "La Prima Che"
```csharp
[Fact]
public async Task AssignRule_WhenTwoPlayersSameTime_OnlyOneSucceeds()
{
    // Arrange
    var gameId = await CreateTestGame();
    var ruleId = await CreateTestRule(gameId);
    var player1Id = await CreateTestPlayer(gameId);
    var player2Id = await CreateTestPlayer(gameId);
    
    // Act - Simulate concurrent requests
    var task1 = _service.AssignRuleAsync(ruleId, gameId, player1Id, ct);
    var task2 = _service.AssignRuleAsync(ruleId, gameId, player2Id, ct);
    
    var results = await Task.WhenAll(task1, task2);
    
    // Assert
    var successes = results.Count(r => r.IsSuccess);
    var conflicts = results.Count(r => r.StatusCode == AppStatusCode.Conflict);
    
    Assert.Equal(1, successes); // ✅ Solo uno vince
    Assert.Equal(1, conflicts); // ✅ Altro riceve 409
}
```

## 📝 Checklist Migrazione

### Repository Layer
- [x] IRepository aggiornato (AppResult, no SaveChanges)
- [x] BaseRepository implementato (Mapster, try-catch, logging)
- [x] GameRepository aggiornato
- [x] PlayerRepository aggiornato
- [x] RuleRepository aggiornato
- [x] RuleAssignmentRepository aggiornato (override AddAsync per "La prima che")

### Service Layer
- [x] GameService aggiornato (end-game logic)
- [x] PlayerService aggiornato
- [x] RuleService aggiornato (check assigned before update)
- [x] RuleAssignmentService aggiornato (transazione atomica)

### Manager Layer
- [x] AuthManager aggiornato
- [x] GameManager aggiornato (transazione creazione gioco)

### Controller Layer
- [ ] Verificare AuthController
- [ ] Verificare GamesController
- [ ] Test endpoint con Postman/Swagger

### Dependency Injection
- [ ] Aggiornare ServiceCollectionExtensions (Infrastructure)
- [ ] Aggiornare ServiceCollectionExtensions (Business)
- [ ] Aggiungere pacchetto Mapster

### Testing
- [ ] Unit test repository
- [ ] Integration test services
- [ ] Race condition test per "La prima che"
- [ ] End-to-end test complete game flow

## 🚀 Deployment Notes

### Database
- Schema rimane invariato
- Nessuna migrazione necessaria
- UNIQUE constraint su RuleAssignmentEntity.RuleId è critico

### Configuration
- Nessuna modifica a appsettings.json
- Jwt configuration rimane uguale

### Performance
- ✅ Auto-save riduce complessità
- ⚠️ Potrebbero esserci più roundtrip DB (uno per operazione)
- ✅ Transazioni esplicite per operazioni multi-step garantiscono atomicità

### Monitoring
- Log strutturati con ILogger
- Trace di tutti gli errori repository/service
- Metriche disponibili per 409 Conflict (race conditions)

## ❗ Breaking Changes

### Per il Team Backend
- ⚠️ **IMPORTANTE**: Non chiamare più `SaveChangesAsync()` manualmente
- ⚠️ Tutti i metodi repository ora ritornano `AppResult<T>`
- ⚠️ Gestire sempre `IsFailure` prima di accedere a `Value`

### Per il Team Frontend
- ✅ Nessun breaking change nelle API
- ✅ Error codes più specifici (es. "RULE_ALREADY_ASSIGNED")
- ✅ Gestione 409 migliorata per race conditions
