using Mangefy.Domain.Menus;
using Mangefy.Domain.Menus.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class MenuRepository : IMenuRepository
{
    private readonly MangefyDbContext _context;
    public MenuRepository(MangefyDbContext context) => _context = context;

    public Task<Menu?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Menus.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Menu?> GetDefaultByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => _context.Menus.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.IsDefault, ct);

    public async Task<IReadOnlyList<Menu>> GetAllByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => await _context.Menus.Where(x => x.TenantId == tenantId).ToListAsync(ct);

    public Task<MenuItem?> GetItemByIdAsync(Guid itemId, CancellationToken ct = default)
        => _context.Menus
            .SelectMany(m => m.Categories)
            .SelectMany(c => c.Items)
            .Cast<MenuItem?>()
            .FirstOrDefaultAsync(i => i != null && i.Id == itemId, ct);

    public async Task<IReadOnlyList<MenuItem>> GetItemsByIdsAsync(IEnumerable<Guid> itemIds, CancellationToken ct = default)
    {
        var ids = itemIds.ToHashSet();
        return await _context.Menus
            .SelectMany(m => m.Categories)
            .SelectMany(c => c.Items)
            .Where(i => ids.Contains(i.Id))
            .ToListAsync(ct);
    }

    public Task<int> CountItemsByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => _context.Menus
            .Where(m => m.TenantId == tenantId)
            .SelectMany(m => m.Categories)
            .SelectMany(c => c.Items)
            .CountAsync(ct);

    public async Task AddAsync(Menu menu, CancellationToken ct = default)
        => await _context.Menus.AddAsync(menu, ct);

    public Task UpdateAsync(Menu menu, CancellationToken ct = default)
    {
        if (_context.Entry(menu).State == EntityState.Detached)
        {
            _context.Menus.Update(menu);
            return Task.CompletedTask;
        }

        // EF Core bug: OwnsMany + client Guid keys → newly-added owned entities are marked
        // Modified (not Added) by DetectChanges because a non-zero Guid looks like an existing
        // row. We work around this by disabling auto-detect and manually promoting new entities.
        //
        // Disable BEFORE the first Entry() call — Entry() itself triggers DetectChanges when
        // AutoDetectChangesEnabled=true.
        //
        // For new entities we use _context.Add() (not just setting State=Added) because Add()
        // traverses the full owned graph and initialises OwnsOne column values (e.g. Price.Amount)
        // so they are not null in the INSERT.
        _context.ChangeTracker.AutoDetectChangesEnabled = false;
        try
        {
            foreach (var cat in menu.Categories)
            {
                var catEntry = _context.Entry(cat);
                if (catEntry.State == EntityState.Detached)
                {
                    _context.Add(cat);
                    continue; // Add() recurses into cat.Items automatically
                }

                foreach (var item in cat.Items)
                {
                    if (_context.Entry(item).State == EntityState.Detached)
                        _context.Add(item);
                }
            }
        }
        finally
        {
            _context.ChangeTracker.AutoDetectChangesEnabled = true;
        }

        return Task.CompletedTask;
    }
}
