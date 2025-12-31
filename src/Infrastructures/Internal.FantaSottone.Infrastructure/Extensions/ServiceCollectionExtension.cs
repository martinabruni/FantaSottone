namespace Internal.FantaSottone.Infrastructure.Extensions;

using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Infrastructure.Models;
using Internal.FantaSottone.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
    {
        // DbContext
        services.AddDbContext<FantaSottoneContext>(options =>
            options.UseSqlServer(connectionString));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IRuleRepository, RuleRepository>();
        services.AddScoped<IRuleAssignmentRepository, RuleAssignmentRepository>();

        return services;
    }
}
