using Mangefy.Domain.Common;

namespace Mangefy.Domain.Tabs.Events;

public sealed class TabOpenedEvent(Guid tabId, Guid tenantId, Guid? tableId, string customerName) : DomainEvent
{
    public Guid TabId { get; } = tabId;
    public Guid TenantId { get; } = tenantId;
    public Guid? TableId { get; } = tableId;
    public string CustomerName { get; } = customerName;
}
