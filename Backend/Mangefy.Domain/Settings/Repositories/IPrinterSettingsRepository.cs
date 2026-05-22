namespace Mangefy.Domain.Settings.Repositories;

public interface IPrinterSettingsRepository
{
    Task<PrinterSettings?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(PrinterSettings settings, CancellationToken cancellationToken = default);
    Task UpdateAsync(PrinterSettings settings, CancellationToken cancellationToken = default);
}
