using Mangefy.Domain.Common;

namespace Mangefy.Domain.Platform.Subscriptions.Events;

public sealed class InvoiceOverdueEvent : DomainEvent
{
    public Guid SubscriptionId { get; }
    public Guid TenantId { get; }
    public Guid InvoiceId { get; }
    public DateOnly DueDate { get; }

    public InvoiceOverdueEvent(Guid subscriptionId, Guid tenantId, Guid invoiceId, DateOnly dueDate)
    {
        SubscriptionId = subscriptionId;
        TenantId = tenantId;
        InvoiceId = invoiceId;
        DueDate = dueDate;
    }
}
