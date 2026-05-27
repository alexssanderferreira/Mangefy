using Mangefy.Application.Platform.Subscriptions.Queries.ListSubscriptions;
using Mangefy.Domain.Platform.Subscriptions;
using Mangefy.Domain.Platform.Subscriptions.Repositories;
using Mangefy.Domain.Plans.Repositories;
using Mangefy.Domain.Tenants.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.Subscriptions.Queries.GetOverdueSubscriptions;

public sealed class GetOverdueSubscriptionsQueryHandler : IRequestHandler<GetOverdueSubscriptionsQuery, IReadOnlyList<SubscriptionDto>>
{
    private readonly ISubscriptionRepository _subscriptions;
    private readonly ITenantRepository _tenants;
    private readonly IPlanRepository _plans;

    public GetOverdueSubscriptionsQueryHandler(
        ISubscriptionRepository subscriptions,
        ITenantRepository tenants,
        IPlanRepository plans)
    {
        _subscriptions = subscriptions;
        _tenants = tenants;
        _plans = plans;
    }

    public async Task<IReadOnlyList<SubscriptionDto>> Handle(GetOverdueSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptions.GetWithOverdueInvoicesAsync(cancellationToken);
        var tenants = await _tenants.GetAllAsync(cancellationToken);
        var plans = await _plans.GetAllAsync(cancellationToken);

        var tenantMap = tenants.ToDictionary(t => t.Id);
        var planMap = plans.ToDictionary(p => p.Id);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return subscriptions.Select(s =>
        {
            // Marca em memória faturas Pending vencidas como Overdue para projeção correta do DTO.
            // Não persiste — o job agendado é responsável pela persistência.
            s.MarkOverdueInvoices(today);

            var tenant = tenantMap.GetValueOrDefault(s.TenantId);
            var plan = planMap.GetValueOrDefault(s.PlanId);
            var latest = s.GetLatestInvoice();
            var overdueCount = s.Invoices.Count(i => i.Status == InvoiceStatus.Overdue);
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
                "Inadimplente");
        }).ToList();
    }
}
