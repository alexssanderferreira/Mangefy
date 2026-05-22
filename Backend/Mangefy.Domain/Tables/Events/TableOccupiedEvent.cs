using Mangefy.Domain.Common;

namespace Mangefy.Domain.Tables.Events;

public sealed class TableOccupiedEvent(Guid tableId, Guid tenantId, string tableNumber) : DomainEvent
{
    public Guid TableId { get; } = tableId;
    public Guid TenantId { get; } = tenantId;
    public string TableNumber { get; } = tableNumber;
}
