using Mangefy.Domain.Platform.Subscriptions;
using Mangefy.Domain.Platform.Subscriptions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class SubscriptionRepository : ISubscriptionRepository
{
    private readonly MangefyDbContext _context;
    public SubscriptionRepository(MangefyDbContext context) => _context = context;

    public Task<Subscription?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Subscriptions.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Subscription?> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default)
        => _context.Subscriptions.FirstOrDefaultAsync(x => x.TenantId == tenantId, ct);

    public async Task<IReadOnlyList<Subscription>> GetAllAsync(CancellationToken ct = default)
        => await _context.Subscriptions.ToListAsync(ct);

    public async Task<IReadOnlyList<Subscription>> GetWithOverdueInvoicesAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var all = await _context.Subscriptions.ToListAsync(ct);
        return all.Where(x => x.HasOverdueInvoices() ||
            x.Invoices.Any(i => i.Status == InvoiceStatus.Pending && i.DueDate < today)).ToList();
    }

    public async Task<IReadOnlyList<Subscription>> GetDueByDateAsync(DateOnly dueDate, CancellationToken ct = default)
        => await _context.Subscriptions.Where(x => x.NextDueDate == dueDate).ToListAsync(ct);

    public Task AddInvoiceAsync(Invoice invoice, CancellationToken ct = default)
    {
        _context.Entry(invoice).State = EntityState.Added;
        return Task.CompletedTask;
    }

    public async Task AddAsync(Subscription subscription, CancellationToken ct = default)
        => await _context.Subscriptions.AddAsync(subscription, ct);

    public Task UpdateAsync(Subscription subscription, CancellationToken ct = default)
    {
        if (_context.Entry(subscription).State == EntityState.Detached)
            _context.Subscriptions.Update(subscription);
        return Task.CompletedTask;
    }
}
