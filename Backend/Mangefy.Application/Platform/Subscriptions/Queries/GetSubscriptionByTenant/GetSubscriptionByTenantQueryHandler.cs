using Mangefy.Application.Platform.Subscriptions.Queries.ListSubscriptions;
using Mangefy.Domain.Platform.Subscriptions;
using Mangefy.Domain.Platform.Subscriptions.Repositories;
using Mangefy.Domain.Plans.Repositories;
using Mangefy.Domain.Tenants.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.Subscriptions.Queries.GetSubscriptionByTenant;

public sealed class GetSubscriptionByTenantQueryHandler : IRequestHandler<GetSubscriptionByTenantQuery, SubscriptionDto?>
{
    private readonly ISubscriptionRepository _subscriptions;
    private readonly ITenantRepository _tenants;
    private readonly IPlanRepository _plans;

    public GetSubscriptionByTenantQueryHandler(
        ISubscriptionRepository subscriptions,
        ITenantRepository tenants,
        IPlanRepository plans)
    {
        _subscriptions = subscriptions;
        _tenants = tenants;
        _plans = plans;
    }

    public async Task<SubscriptionDto?> Handle(GetSubscriptionByTenantQuery request, CancellationToken cancellationToken)
    {
        var s = await _subscriptions.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (s is null) return null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        s.MarkOverdueInvoices(today);

        var tenant = await _tenants.GetByIdAsync(request.TenantId, cancellationToken);
        var plan = await _plans.GetByIdAsync(s.PlanId, cancellationToken);

        var overdueCount = s.Invoices.Count(i => i.Status == InvoiceStatus.Overdue);
        var latest = s.GetLatestInvoice();

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
            s.Invoices
                .OrderByDescending(i => i.DueDate)
                .Select(i => new InvoiceDto(
                    i.Id,
                    i.Amount.Amount,
                    i.Amount.Currency,
                    i.DueDate,
                    i.PaidAt,
                    i.Status.ToString(),
                    i.PaymentReference,
                    i.Notes)).ToList(),
            status);
    }
}
