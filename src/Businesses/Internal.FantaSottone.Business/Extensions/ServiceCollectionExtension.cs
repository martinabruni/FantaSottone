namespace Internal.FantaSottone.Business.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Internal.FantaSottone.Business.Validators;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<GameValidator>();
        services.AddScoped<PlayerValidator>();
        services.AddScoped<RuleValidator>();
        services.AddScoped<RuleAssignmentValidator>();
        return services;
    }
}