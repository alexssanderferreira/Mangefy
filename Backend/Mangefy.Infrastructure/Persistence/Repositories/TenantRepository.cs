using Mangefy.Domain.Tenants;
using Mangefy.Domain.Tenants.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly MangefyDbContext _context;
    public TenantRepository(MangefyDbContext context) => _context = context;

    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Tenants.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => _context.Tenants.FirstOrDefaultAsync(x => x.Slug == slug, ct);

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct = default)
        => _context.Tenants.AnyAsync(x => x.Slug == slug, ct);

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default)
        => await _context.Tenants.ToListAsync(ct);

    public async Task<(IReadOnlyList<Tenant> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Tenants.OrderBy(t => t.Name);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<Dictionary<Guid, int>> CountByBusinessTypeAsync(CancellationToken ct = default)
        => await _context.Tenants
            .GroupBy(t => t.BusinessTypeId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

    public Task<int> CountByOwnerAsync(Guid ownerId, CancellationToken ct = default)
        => _context.Tenants.CountAsync(x => x.OwnerId == ownerId, ct);

    public async Task<IReadOnlyList<Tenant>> GetByOwnerAsync(Guid ownerId, CancellationToken ct = default)
        => await _context.Tenants.Where(x => x.OwnerId == ownerId).OrderBy(x => x.Name).ToListAsync(ct);

    public async Task AddAsync(Tenant tenant, CancellationToken ct = default)
        => await _context.Tenants.AddAsync(tenant, ct);

    public Task UpdateAsync(Tenant tenant, CancellationToken ct = default)
    {
        _context.Tenants.Update(tenant);
        return Task.CompletedTask;
    }
}
