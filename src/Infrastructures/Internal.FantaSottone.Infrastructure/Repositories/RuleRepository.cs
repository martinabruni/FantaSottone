namespace Internal.FantaSottone.Infrastructure.Repositories;

using Internal.FantaSottone.Domain.Models;
using Internal.FantaSottone.Domain.Repositories;
using Internal.FantaSottone.Domain.Results;
using Internal.FantaSottone.Infrastructure.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

internal sealed class RuleRepository : BaseRepository<Rule, RuleEntity, int>, IRuleRepository
{
    public RuleRepository(FantaSottoneContext context, ILogger<RuleRepository> logger)
        : base(context, logger)
    {
    }

    public async Task<AppResult<IEnumerable<Rule>>> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _context.RuleEntity
                .Where(r => r.GameId == gameId)
                .ToListAsync(cancellationToken);

            var domainEntities = entities.Adapt<IEnumerable<Rule>>();
            return AppResult<IEnumerable<Rule>>.Success(domainEntities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rules for game {GameId}", gameId);
            return AppResult<IEnumerable<Rule>>.InternalServerError($"Error retrieving rules: {ex.Message}");
        }
    }

    public async Task<AppResult<int>> CountByGameIdAsync(int gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await _context.RuleEntity
                .Where(r => r.GameId == gameId)
                .CountAsync(cancellationToken);

            return AppResult<int>.Success(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting rules for game {GameId}", gameId);
            return AppResult<int>.InternalServerError($"Error counting rules: {ex.Message}");
        }
    }
}
