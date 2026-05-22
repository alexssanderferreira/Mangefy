namespace Mangefy.Domain.Platform.SupplierCategories.Repositories;

public interface ISupplierCategoryRepository
{
    Task<SupplierCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SupplierCategory>> GetGlobalAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SupplierCategory>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SupplierCategory>> GetAllGlobalAsync(CancellationToken cancellationToken = default);
    Task AddAsync(SupplierCategory category, CancellationToken cancellationToken = default);
    Task UpdateAsync(SupplierCategory category, CancellationToken cancellationToken = default);
    Task DeleteAsync(SupplierCategory category, CancellationToken cancellationToken = default);
}
