using Mangefy.Domain.Platform.Subscriptions;
using Mangefy.Domain.Platform.Subscriptions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mangefy.Infrastructure.Persistence.Repositories;

public sealed class SubscriptionRepository : ISubscriptionRepository
{
    private readonly MangefyDbContext _context;
    public SubscriptionRepository(MangefyDbContext context) => _context = context;

    public Task<Subscription?> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default)
        => _context.Subscriptions.FirstOrDefaultAsync(x => x.TenantId == tenantId, ct);

    public async Task<IReadOnlyList<Subscription>> GetWithOverdueInvoicesAsync(CancellationToken ct = default)
        => await _context.Subscriptions.Where(x => x.HasOverdueInvoices()).ToListAsync(ct);

    public async Task<IReadOnlyList<Subscription>> GetDueByDateAsync(DateOnly dueDate, CancellationToken ct = default)
        => await _context.Subscriptions.Where(x => x.NextDueDate == dueDate).ToListAsync(ct);

    public async Task AddAsync(Subscription subscription, CancellationToken ct = default)
        => await _context.Subscriptions.AddAsync(subscription, ct);

    public Task UpdateAsync(Subscription subscription, CancellationToken ct = default)
    {
        if (_context.Entry(subscription).State == EntityState.Detached)
            _context.Subscriptions.Update(subscription);
        return Task.CompletedTask;
    }
}
