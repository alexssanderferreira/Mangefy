namespace Mangefy.Domain.Settings.Repositories;

public interface IIntegrationSettingsRepository
{
    Task<IntegrationSettings?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(IntegrationSettings settings, CancellationToken cancellationToken = default);
    Task UpdateAsync(IntegrationSettings settings, CancellationToken cancellationToken = default);
}
