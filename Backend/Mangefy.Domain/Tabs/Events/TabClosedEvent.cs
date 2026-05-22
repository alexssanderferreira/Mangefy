using Mangefy.Domain.Common;

namespace Mangefy.Domain.Tabs.Events;

public sealed class TabClosedEvent(Guid tabId, Guid tenantId, Guid? tableId, decimal totalAmount) : DomainEvent
{
    public Guid TabId { get; } = tabId;
    public Guid TenantId { get; } = tenantId;
    public Guid? TableId { get; } = tableId;
    public decimal TotalAmount { get; } = totalAmount;
}
