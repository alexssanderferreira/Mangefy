using Mangefy.Domain.Common;

namespace Mangefy.Domain.Tabs.Events;

public sealed class TabCancelledEvent(Guid tabId, Guid tenantId, Guid? tableId, string reason) : DomainEvent
{
    public Guid TabId { get; } = tabId;
    public Guid TenantId { get; } = tenantId;
    public Guid? TableId { get; } = tableId;
    public string Reason { get; } = reason;
}
