namespace Mangefy.Domain.Menus.Repositories;

public interface IMenuRepository
{
    Task<Menu?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Menu?> GetDefaultByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<Menu>> GetAllByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<MenuItem?> GetItemByIdAsync(Guid itemId, CancellationToken ct = default);
    Task<IReadOnlyList<MenuItem>> GetItemsByIdsAsync(IEnumerable<Guid> itemIds, CancellationToken ct = default);
    Task<int> CountItemsByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(Menu menu, CancellationToken ct = default);
    Task UpdateAsync(Menu menu, CancellationToken ct = default);
}
