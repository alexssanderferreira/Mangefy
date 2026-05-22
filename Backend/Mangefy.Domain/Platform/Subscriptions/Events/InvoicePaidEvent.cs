using Mangefy.Domain.Common;

namespace Mangefy.Domain.Platform.Subscriptions.Events;

public sealed class InvoicePaidEvent : DomainEvent
{
    public Guid SubscriptionId { get; }
    public Guid TenantId { get; }
    public Guid InvoiceId { get; }
    public decimal Amount { get; }
    public DateOnly PaidAt { get; }

    public InvoicePaidEvent(Guid subscriptionId, Guid tenantId, Guid invoiceId, decimal amount, DateOnly paidAt)
    {
        SubscriptionId = subscriptionId;
        TenantId = tenantId;
        InvoiceId = invoiceId;
        Amount = amount;
        PaidAt = paidAt;
    }
}
