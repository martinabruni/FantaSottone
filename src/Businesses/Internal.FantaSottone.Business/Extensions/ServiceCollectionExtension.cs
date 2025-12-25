namespace Internal.FantaSottone.Business.Extensions;

using Internal.FantaSottone.Business.Managers;
using Internal.FantaSottone.Business.Services;
using Internal.FantaSottone.Business.Validators;
using Internal.FantaSottone.Domain.Managers;
using Internal.FantaSottone.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // Validators
        services.AddScoped<GameValidator>();
        services.AddScoped<PlayerValidator>();
        services.AddScoped<RuleValidator>();
        services.AddScoped<RuleAssignmentValidator>();

        // Services
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<IPlayerService, PlayerService>();
        services.AddScoped<IRuleService, RuleService>();
        services.AddScoped<IRuleAssignmentService, RuleAssignmentService>();

        // Managers
        services.AddScoped<IGameManager, GameManager>();
        services.AddScoped<IAuthManager, AuthManager>();

        return services;
    }
}
