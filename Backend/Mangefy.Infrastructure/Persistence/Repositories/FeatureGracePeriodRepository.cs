using Mangefy.Domain.Platform.Features;
using Mangefy.Domain.Platform.Features.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class FeatureGracePeriodRepository : IFeatureGracePeriodRepository
{
    private readonly MangefyDbContext _context;

    public FeatureGracePeriodRepository(MangefyDbContext context) => _context = context;

    public Task<FeatureGracePeriod?> GetByTenantAndFeatureAsync(Guid tenantId, string featureKey, CancellationToken ct = default)
        => _context.FeatureGracePeriods.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.FeatureKey == featureKey && x.ExpiresAt > DateTime.UtcNow, ct);

    public async Task<IReadOnlyList<FeatureGracePeriod>> GetActiveByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => await _context.FeatureGracePeriods
            .Where(x => x.TenantId == tenantId && x.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<FeatureGracePeriod>> GetExpiredAsync(CancellationToken ct = default)
        => await _context.FeatureGracePeriods.Where(x => x.ExpiresAt <= DateTime.UtcNow).ToListAsync(ct);

    public async Task AddAsync(FeatureGracePeriod gracePeriod, CancellationToken ct = default)
        => await _context.FeatureGracePeriods.AddAsync(gracePeriod, ct);

    public Task UpdateAsync(FeatureGracePeriod gracePeriod, CancellationToken ct = default)
    {
        _context.FeatureGracePeriods.Update(gracePeriod);
        return Task.CompletedTask;
    }
}
