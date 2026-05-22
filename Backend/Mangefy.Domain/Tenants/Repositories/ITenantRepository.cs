namespace Mangefy.Domain.Tenants.Repositories;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct = default);
    Task AddAsync(Tenant tenant, CancellationToken ct = default);
    Task UpdateAsync(Tenant tenant, CancellationToken ct = default);
    Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default);
    Task<(IReadOnlyList<Tenant> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<Dictionary<Guid, int>> CountByBusinessTypeAsync(CancellationToken ct = default);
    Task<int> CountByOwnerAsync(Guid ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<Tenant>> GetByOwnerAsync(Guid ownerId, CancellationToken ct = default);
}
