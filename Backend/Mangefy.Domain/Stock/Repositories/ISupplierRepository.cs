namespace Mangefy.Domain.Stock.Repositories;

public interface ISupplierRepository
{
    Task<Supplier?> GetByIdAsync(Guid tenantId, Guid supplierId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Supplier>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default);
    Task UpdateAsync(Supplier supplier, CancellationToken cancellationToken = default);
}
