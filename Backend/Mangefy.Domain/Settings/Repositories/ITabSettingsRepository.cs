namespace Mangefy.Domain.Settings.Repositories;

public interface ITabSettingsRepository
{
    Task<TabSettings?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(TabSettings settings, CancellationToken cancellationToken = default);
    Task UpdateAsync(TabSettings settings, CancellationToken cancellationToken = default);
}
