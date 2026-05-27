using Mangefy.Domain.Platform.Subscriptions;
using Mangefy.Domain.Platform.Subscriptions.Repositories;
using Mangefy.Domain.Plans.Repositories;
using Mangefy.Domain.Tenants.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.Subscriptions.Queries.ListSubscriptions;

public sealed class ListSubscriptionsQueryHandler : IRequestHandler<ListSubscriptionsQuery, IReadOnlyList<SubscriptionDto>>
{
    private readonly ISubscriptionRepository _subscriptions;
    private readonly ITenantRepository _tenants;
    private readonly IPlanRepository _plans;

    public ListSubscriptionsQueryHandler(
        ISubscriptionRepository subscriptions,
        ITenantRepository tenants,
        IPlanRepository plans)
    {
        _subscriptions = subscriptions;
        _tenants = tenants;
        _plans = plans;
    }

    public async Task<IReadOnlyList<SubscriptionDto>> Handle(ListSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptions.GetAllAsync(cancellationToken);
        var tenants = await _tenants.GetAllAsync(cancellationToken);
        var plans = await _plans.GetAllAsync(cancellationToken);

        var tenantMap = tenants.ToDictionary(t => t.Id);
        var planMap = plans.ToDictionary(p => p.Id);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return subscriptions.Select(s =>
        {
            s.MarkOverdueInvoices(today);

            var tenant = tenantMap.GetValueOrDefault(s.TenantId);
            var plan = planMap.GetValueOrDefault(s.PlanId);
            var latest = s.GetLatestInvoice();
            var overdueCount = s.Invoices.Count(i => i.Status == InvoiceStatus.Overdue);

            var status = overdueCount > 0
                ? "Inadimplente"
                : s.Invoices.Any(i => i.Status == InvoiceStatus.Pending)
                    ? "AguardandoPagamento"
                    : latest?.Status == InvoiceStatus.Paid
                        ? "EmDia"
                        : "SemFaturas";

            return new SubscriptionDto(
                s.Id,
                s.TenantId,
                tenant?.Name ?? "–",
                tenant?.Slug ?? "–",
                s.PlanId,
                plan?.Name ?? "–",
                s.StartDate,
                s.NextDueDate,
                latest?.Status.ToString(),
                latest?.Amount.Amount,
                latest?.DueDate,
                overdueCount,
                s.Invoices.Select(i => new InvoiceDto(
                    i.Id,
                    i.Amount.Amount,
                    i.Amount.Currency,
                    i.DueDate,
                    i.PaidAt,
                    i.Status.ToString(),
                    i.PaymentReference,
                    i.Notes)).ToList(),
                status);
        }).ToList();
    }
}
