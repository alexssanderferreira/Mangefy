using Mangefy.Domain.Common;

namespace Mangefy.Domain.Stock.Events;

public sealed class StockLowEvent : DomainEvent
{
    public Guid TenantId { get; }
    public Guid StockItemId { get; }
    public string ItemName { get; }
    public decimal CurrentQuantity { get; }
    public decimal MinimumQuantity { get; }

    public StockLowEvent(Guid tenantId, Guid stockItemId, string itemName, decimal currentQuantity, decimal minimumQuantity)
    {
        TenantId = tenantId;
        StockItemId = stockItemId;
        ItemName = itemName;
        CurrentQuantity = currentQuantity;
        MinimumQuantity = minimumQuantity;
    }
}
