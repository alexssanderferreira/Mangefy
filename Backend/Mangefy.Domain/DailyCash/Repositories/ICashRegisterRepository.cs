namespace Mangefy.Domain.DailyCash.Repositories;

public interface ICashRegisterRepository
{
    Task<CashRegister?> GetOpenByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<CashRegister?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CashRegister>> GetHistoryByTenantAsync(Guid tenantId, DateOnly from, DateOnly to, CancellationToken ct = default);
    Task AddAsync(CashRegister register, CancellationToken ct = default);
    Task UpdateAsync(CashRegister register, CancellationToken ct = default);
}
