namespace Mangefy.Domain.Platform.Suppliers.Repositories;

public interface IPlatformSupplierRepository
{
    Task<PlatformSupplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlatformSupplier>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlatformSupplier>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlatformSupplier>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task AddAsync(PlatformSupplier supplier, CancellationToken cancellationToken = default);
    Task UpdateAsync(PlatformSupplier supplier, CancellationToken cancellationToken = default);
    Task DeleteAsync(PlatformSupplier supplier, CancellationToken cancellationToken = default);
}
