namespace Mangefy.Domain.Tabs.Repositories;

public interface ITabRepository
{
    Task<Tab?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Tab>> GetOpenByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<Tab>> GetOpenByTableAsync(Guid tenantId, Guid tableId, CancellationToken ct = default);
    Task<IReadOnlyList<Tab>> GetKdsPendingAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<Tab>> GetClosedByPeriodAsync(Guid tenantId, DateTime from, DateTime to, CancellationToken ct = default);
    Task<int> GetNextNumberAsync(Guid tenantId, CancellationToken ct = default);
    Task<int?> GetNextAvailableNumberAsync(Guid tenantId, int min, int max, CancellationToken ct = default);
    Task AddAsync(Tab tab, CancellationToken ct = default);
    Task UpdateAsync(Tab tab, CancellationToken ct = default);
}
