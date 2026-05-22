using Mangefy.Domain.Roles;
using Mangefy.Domain.Roles.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class TenantRoleRepository : ITenantRoleRepository
{
    private readonly MangefyDbContext _context;
    public TenantRoleRepository(MangefyDbContext context) => _context = context;

    public Task<TenantRole?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.TenantRoles.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<TenantRole>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => await _context.TenantRoles.Where(x => x.TenantId == tenantId).ToListAsync(ct);

    public Task<TenantRole?> GetOwnerRoleByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => _context.TenantRoles.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.IsOwnerRole, ct);

    public Task<bool> ExistsByNameAsync(Guid tenantId, string name, CancellationToken ct = default)
        => _context.TenantRoles.AnyAsync(x => x.TenantId == tenantId && x.Name == name, ct);

    public async Task AddAsync(TenantRole role, CancellationToken ct = default)
        => await _context.TenantRoles.AddAsync(role, ct);

    public Task UpdateAsync(TenantRole role, CancellationToken ct = default)
    {
        _context.TenantRoles.Update(role);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var role = _context.TenantRoles.Local.FirstOrDefault(x => x.Id == id);
        if (role is not null) _context.TenantRoles.Remove(role);
        return Task.CompletedTask;
    }

    public async Task<Dictionary<Guid, int>> CountByTemplateIdAsync(CancellationToken ct = default)
        => await _context.TenantRoles
            .Where(r => r.TemplateId != null)
            .GroupBy(r => r.TemplateId!.Value)
            .Select(g => new { Key = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
}
