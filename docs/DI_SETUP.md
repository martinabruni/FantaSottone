# Dependency Injection & NuGet Packages - Setup Completo

## 📦 Pacchetti NuGet da Aggiungere

### Internal.FantaSottone.Infrastructure.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- EF Core (già presente) -->
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.11" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.11" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.11">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    
    <!-- 🆕 MAPSTER - AGGIUNGI QUESTI -->
    <PackageReference Include="Mapster" Version="7.4.0" />
    <PackageReference Include="Mapster.DependencyInjection" Version="1.0.1" />
    
    <!-- Logging -->
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\Domains\Internal.FantaSottone.Domain\Internal.FantaSottone.Domain.csproj" />
  </ItemGroup>
</Project>
```

---

## 🔧 ServiceCollectionExtensions - Infrastructure

### File: `src/Infrastructures/Internal.FantaSottone.Infrastructure/Extensions/ServiceCollectionExtensions.cs`

```csharp
namespace Internal.FantaSottone.Infrastructure.Extensions;

using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Infrastructure.Models;
using Internal.FantaSottone.Infrastructure.Repositories;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        string connectionString)
    {
        // DbContext
        services.AddDbContext<FantaSottoneContext>(options =>
            options.UseSqlServer(connectionString));

        // 🆕 MAPSTER CONFIGURATION
        // Scan current assembly for IRegister implementations
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());
        
        // Register mapper
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        // 🆕 REPOSITORIES CON LOGGING
        // Nota: BaseRepository richiede ILogger<T>, quindi ogni repository concreto
        // deve essere registrato esplicitamente (non possiamo usare generic registration)
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IRuleRepository, RuleRepository>();
        services.AddScoped<IRuleAssignmentRepository, RuleAssignmentRepository>();

        return services;
    }
}
```

**Nota**: Se vuoi configurazioni custom di Mapster, crea una classe `MapsterConfiguration.cs`:

```csharp
namespace Internal.FantaSottone.Infrastructure.Mappers;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Infrastructure.Models;
using Mapster;

public class MapsterConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Auto-mapping predefinito è OK, ma se servono custom:
        
        // Esempio: Mapping custom per Game
        config.NewConfig<GameEntity, Game>()
            .Map(dest => dest.Status, src => (GameStatus)src.Status);
        
        config.NewConfig<Game, GameEntity>()
            .Map(dest => dest.Status, src => (byte)src.Status);
        
        // Esempio: Mapping custom per Player
        config.NewConfig<PlayerEntity, Player>();
        config.NewConfig<Player, PlayerEntity>();
        
        // Esempio: Mapping custom per Rule
        config.NewConfig<RuleEntity, Rule>()
            .Map(dest => dest.RuleType, src => (RuleType)src.RuleType);
        
        config.NewConfig<Rule, RuleEntity>()
            .Map(dest => dest.RuleType, src => (byte)src.RuleType);
        
        // Esempio: Mapping custom per RuleAssignment
        config.NewConfig<RuleAssignmentEntity, RuleAssignment>();
        config.NewConfig<RuleAssignment, RuleAssignmentEntity>();
    }
}
```

---

## 🔧 ServiceCollectionExtensions - Business

### File: `src/Businesses/Internal.FantaSottone.Business/Extensions/ServiceCollectionExtensions.cs`

**Nessuna modifica necessaria** - Il file esistente è già corretto:

```csharp
namespace Internal.FantaSottone.Business.Extensions;

using Internal.FantaSottone.Business.Managers;
using Internal.FantaSottone.Business.Services;
using Internal.FantaSottone.Business.Validators;
using Internal.FantaSottone.Domain.Managers;
using Internal.FantaSottone.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // Validators (già presente)
        services.AddScoped<GameValidator>();
        services.AddScoped<PlayerValidator>();
        services.AddScoped<RuleValidator>();
        services.AddScoped<RuleAssignmentValidator>();

        // Services (già presente)
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<IPlayerService, PlayerService>();
        services.AddScoped<IRuleService, RuleService>();
        services.AddScoped<IRuleAssignmentService, RuleAssignmentService>();

        // Managers (già presente)
        services.AddScoped<IGameManager, GameManager>();
        services.AddScoped<IAuthManager, AuthManager>();

        return services;
    }
}
```

---

## 🔧 Program.cs - API

### File: `src/Apis/Internal.FantaSottone.Api/Program.cs`

**Nessuna modifica necessaria** - Il file esistente è già corretto:

```csharp
using Internal.FantaSottone.Business.Extensions;
using Internal.FantaSottone.Infrastructure.Extensions;
// ... altri using

var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");

// ✅ Add layers services (già presente)
builder.Services.AddInfrastructureServices(connectionString); // ✅ Mapster registrato qui
builder.Services.AddBusinessServices();

// ✅ Controllers, JWT, Swagger, ecc. (già presente)
// ...

var app = builder.Build();

// ✅ Pipeline (già presente)
// ...

app.Run();
```

---

## 🧪 Verifica Registrazione

Aggiungi questo controller di diagnostica (solo in Development):

```csharp
#if DEBUG
[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public DiagnosticsController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("registrations")]
    public IActionResult GetRegistrations()
    {
        var services = new List<string>();

        // Check repositories
        try
        {
            var gameRepo = _serviceProvider.GetService<IGameRepository>();
            services.Add($"IGameRepository: {(gameRepo != null ? "✅ Registered" : "❌ Missing")}");
        }
        catch (Exception ex)
        {
            services.Add($"IGameRepository: ❌ Error - {ex.Message}");
        }

        // Check services
        try
        {
            var gameService = _serviceProvider.GetService<IGameService>();
            services.Add($"IGameService: {(gameService != null ? "✅ Registered" : "❌ Missing")}");
        }
        catch (Exception ex)
        {
            services.Add($"IGameService: ❌ Error - {ex.Message}");
        }

        // Check managers
        try
        {
            var authManager = _serviceProvider.GetService<IAuthManager>();
            services.Add($"IAuthManager: {(authManager != null ? "✅ Registered" : "❌ Missing")}");
        }
        catch (Exception ex)
        {
            services.Add($"IAuthManager: ❌ Error - {ex.Message}");
        }

        // Check DbContext
        try
        {
            var context = _serviceProvider.GetService<FantaSottoneContext>();
            services.Add($"FantaSottoneContext: {(context != null ? "✅ Registered" : "❌ Missing")}");
        }
        catch (Exception ex)
        {
            services.Add($"FantaSottoneContext: ❌ Error - {ex.Message}");
        }

        // Check Mapster
        try
        {
            var mapper = _serviceProvider.GetService<IMapper>();
            services.Add($"IMapper (Mapster): {(mapper != null ? "✅ Registered" : "❌ Missing")}");
        }
        catch (Exception ex)
        {
            services.Add($"IMapper (Mapster): ❌ Error - {ex.Message}");
        }

        return Ok(new { Services = services });
    }
}
#endif
```

Test con: `GET https://localhost:7017/api/diagnostics/registrations`

Expected output:
```json
{
  "services": [
    "IGameRepository: ✅ Registered",
    "IGameService: ✅ Registered",
    "IAuthManager: ✅ Registered",
    "FantaSottoneContext: ✅ Registered",
    "IMapper (Mapster): ✅ Registered"
  ]
}
```

---

## 📝 Checklist Setup

- [ ] Aggiungere pacchetto `Mapster` a Infrastructure.csproj
- [ ] Aggiungere pacchetto `Mapster.DependencyInjection` a Infrastructure.csproj
- [ ] Aggiornare `ServiceCollectionExtensions.cs` in Infrastructure (aggiungere Mapster config)
- [ ] (Opzionale) Creare `MapsterConfiguration.cs` per custom mappings
- [ ] Verificare che `Program.cs` chiami `AddInfrastructureServices()`
- [ ] Verificare che `Program.cs` chiami `AddBusinessServices()`
- [ ] Build soluzione → Verificare nessun errore di compilazione
- [ ] Run API → Verificare nessun errore di startup
- [ ] (Dev only) Test endpoint `/api/diagnostics/registrations`
- [ ] Test endpoint reali (`/api/auth/login`, `/api/games/start`)

---

## ⚠️ Troubleshooting

### Errore: "Unable to resolve service for type 'ILogger<GameRepository>'"

**Causa**: Logging non configurato correttamente in Program.cs

**Soluzione**: Verifica che Program.cs abbia:
```csharp
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
```

O usa il default (già incluso in `CreateBuilder`).

---

### Errore: "No parameterless constructor defined for 'BaseRepository'"

**Causa**: BaseRepository è abstract, non può essere istanziato direttamente

**Soluzione**: Verifica che stai registrando le implementazioni concrete:
```csharp
services.AddScoped<IGameRepository, GameRepository>(); // ✅ Corretto
// NON: services.AddScoped<IGameRepository, BaseRepository<...>>(); // ❌ Errore
```

---

### Errore: Mapster mapping fails at runtime

**Causa**: Configurazione Mapster non registrata

**Soluzione**: Verifica che `ServiceCollectionExtensions` chiami:
```csharp
var config = TypeAdapterConfig.GlobalSettings;
config.Scan(Assembly.GetExecutingAssembly());
services.AddSingleton(config);
services.AddScoped<IMapper, ServiceMapper>();
```

---

### Errore: "A second operation started on this context before a previous operation completed"

**Causa**: DbContext condiviso tra scope o chiamate concurrent

**Soluzione**: 
1. Verifica che DbContext sia registrato come `Scoped` (default)
2. Non condividere istanze di repository/service tra request
3. In test, usa un nuovo DbContext per ogni test case

---

## ✅ Verification Script

```bash
# PowerShell script to verify packages and files

Write-Host "Checking Infrastructure project..."
$infraCsproj = Get-Content "src/Infrastructures/Internal.FantaSottone.Infrastructure/Internal.FantaSottone.Infrastructure.csproj"

if ($infraCsproj -like "*Mapster*") {
    Write-Host "✅ Mapster package found" -ForegroundColor Green
} else {
    Write-Host "❌ Mapster package NOT found" -ForegroundColor Red
}

Write-Host "`nChecking ServiceCollectionExtensions..."
$extensions = Get-Content "src/Infrastructures/Internal.FantaSottone.Infrastructure/Extensions/ServiceCollectionExtensions.cs"

if ($extensions -like "*AddMapster*" -or $extensions -like "*TypeAdapterConfig*") {
    Write-Host "✅ Mapster configuration found" -ForegroundColor Green
} else {
    Write-Host "❌ Mapster configuration NOT found" -ForegroundColor Red
}

Write-Host "`nChecking repositories..."
$repos = @("GameRepository", "PlayerRepository", "RuleRepository", "RuleAssignmentRepository")
foreach ($repo in $repos) {
    $file = "src/Infrastructures/Internal.FantaSottone.Infrastructure/Repositories/$repo.cs"
    if (Test-Path $file) {
        Write-Host "✅ $repo exists" -ForegroundColor Green
    } else {
        Write-Host "❌ $repo NOT found" -ForegroundColor Red
    }
}

Write-Host "`n✅ Verification complete!"
```

---

## 🚀 Final Deploy Checklist

- [ ] All packages installed
- [ ] Build succeeds
- [ ] No startup errors
- [ ] Diagnostics endpoint returns all ✅
- [ ] Login endpoint works
- [ ] Start game endpoint works
- [ ] Assign rule endpoint works (including 409 on race condition)
- [ ] End game endpoint works
- [ ] All integration tests pass
