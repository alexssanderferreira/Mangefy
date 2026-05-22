namespace Mangefy.Domain.Settings.Repositories;

public interface IFiscalSettingsRepository
{
    Task<FiscalSettings?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(FiscalSettings settings, CancellationToken cancellationToken = default);
    Task UpdateAsync(FiscalSettings settings, CancellationToken cancellationToken = default);
}
