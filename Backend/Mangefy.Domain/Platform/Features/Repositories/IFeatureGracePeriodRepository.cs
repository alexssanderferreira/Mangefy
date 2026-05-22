namespace Mangefy.Domain.Platform.Features.Repositories;

public interface IFeatureGracePeriodRepository
{
    Task<FeatureGracePeriod?> GetByTenantAndFeatureAsync(Guid tenantId, string featureKey, CancellationToken ct = default);
    Task<IReadOnlyList<FeatureGracePeriod>> GetActiveByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<FeatureGracePeriod>> GetExpiredAsync(CancellationToken ct = default);
    Task AddAsync(FeatureGracePeriod gracePeriod, CancellationToken ct = default);
    Task UpdateAsync(FeatureGracePeriod gracePeriod, CancellationToken ct = default);
}
