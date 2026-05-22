namespace Mangefy.Domain.Roles.Repositories;

public interface ITenantRoleRepository
{
    Task<TenantRole?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TenantRole>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<TenantRole?> GetOwnerRoleByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(Guid tenantId, string name, CancellationToken ct = default);
    Task AddAsync(TenantRole role, CancellationToken ct = default);
    Task UpdateAsync(TenantRole role, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<Dictionary<Guid, int>> CountByTemplateIdAsync(CancellationToken ct = default);
}
