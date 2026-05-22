namespace Mangefy.Domain.Tables.Repositories;

public interface ITableRepository
{
    Task<Table?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Table>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<Table?> GetByNumberAsync(Guid tenantId, string number, CancellationToken ct = default);
    Task<bool> ExistsByNumberAsync(Guid tenantId, string number, CancellationToken ct = default);
    Task<int> CountByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(Table table, CancellationToken ct = default);
    Task UpdateAsync(Table table, CancellationToken ct = default);
}
