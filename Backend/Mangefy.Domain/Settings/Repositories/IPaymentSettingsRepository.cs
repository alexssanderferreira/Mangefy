namespace Mangefy.Domain.Settings.Repositories;

public interface IPaymentSettingsRepository
{
    Task<PaymentSettings?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(PaymentSettings settings, CancellationToken cancellationToken = default);
    Task UpdateAsync(PaymentSettings settings, CancellationToken cancellationToken = default);
}
