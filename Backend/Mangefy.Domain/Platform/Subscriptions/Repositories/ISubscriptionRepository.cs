namespace Mangefy.Domain.Platform.Subscriptions.Repositories;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<Subscription>> GetWithOverdueInvoicesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Subscription>> GetDueByDateAsync(DateOnly dueDate, CancellationToken ct = default);
    Task AddAsync(Subscription subscription, CancellationToken ct = default);
    Task UpdateAsync(Subscription subscription, CancellationToken ct = default);
}
