using Mangefy.Domain.Tabs;
using Mangefy.Domain.Tabs.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class TabRepository : ITabRepository
{
    private readonly MangefyDbContext _context;
    public TabRepository(MangefyDbContext context) => _context = context;

    public Task<Tab?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Tabs.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Tab>> GetOpenByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => await _context.Tabs.Where(x => x.TenantId == tenantId && x.Status == TabStatus.Open).ToListAsync(ct);

    public async Task<IReadOnlyList<Tab>> GetOpenByTableAsync(Guid tenantId, Guid tableId, CancellationToken ct = default)
        => await _context.Tabs.Where(x => x.TenantId == tenantId && x.CurrentTableId == tableId && x.Status == TabStatus.Open).ToListAsync(ct);

    public async Task<IReadOnlyList<Tab>> GetKdsPendingAsync(Guid tenantId, CancellationToken ct = default)
        => await _context.Tabs.Where(x => x.TenantId == tenantId && x.Status == TabStatus.Open).ToListAsync(ct);

    public async Task<IReadOnlyList<Tab>> GetClosedByPeriodAsync(Guid tenantId, DateTime from, DateTime to, CancellationToken ct = default)
        => await _context.Tabs
            .Where(x => x.TenantId == tenantId && x.ClosedAt >= from && x.ClosedAt <= to)
            .ToListAsync(ct);

    public async Task<int> GetNextNumberAsync(Guid tenantId, CancellationToken ct = default)
    {
        var maxNumber = await _context.Tabs
            .Where(x => x.TenantId == tenantId)
            .MaxAsync(x => (int?)x.Number, ct);
        return (maxNumber ?? 0) + 1;
    }

    public async Task<int?> GetNextAvailableNumberAsync(Guid tenantId, int min, int max, CancellationToken ct = default)
    {
        // Números atualmente em uso por comandas abertas
        var usedNumbers = (await _context.Tabs
            .Where(x => x.TenantId == tenantId && x.Status == TabStatus.Open)
            .Select(x => x.Number)
            .ToListAsync(ct)).ToHashSet();

        for (int n = min; n <= max; n++)
        {
            if (!usedNumbers.Contains(n))
                return n;
        }

        return null;
    }

    public async Task AddAsync(Tab tab, CancellationToken ct = default)
        => await _context.Tabs.AddAsync(tab, ct);

    public Task UpdateAsync(Tab tab, CancellationToken ct = default)
    {
        _context.ChangeTracker.AutoDetectChangesEnabled = false;
        try
        {
            if (_context.Entry(tab).State == EntityState.Detached)
            {
                _context.Tabs.Update(tab);
                return Task.CompletedTask;
            }

            foreach (var order in tab.Orders)
            {
                if (_context.Entry(order).State == EntityState.Detached)
                    _context.Entry(order).State = EntityState.Added;
                foreach (var item in order.Items)
                {
                    if (_context.Entry(item).State == EntityState.Detached)
                        _context.Entry(item).State = EntityState.Added;
                }
            }

            foreach (var payment in tab.Payments)
            {
                if (_context.Entry(payment).State == EntityState.Detached)
                    _context.Entry(payment).State = EntityState.Added;
            }
        }
        finally
        {
            _context.ChangeTracker.AutoDetectChangesEnabled = true;
        }

        return Task.CompletedTask;
    }
}
