namespace Mangefy.Domain.Settings.Repositories;

public interface IWorkforceSettingsRepository
{
    Task<WorkforceSettings?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(WorkforceSettings settings, CancellationToken cancellationToken = default);
    Task UpdateAsync(WorkforceSettings settings, CancellationToken cancellationToken = default);
}
