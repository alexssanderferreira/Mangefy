using Mangefy.Domain.Tabs;

namespace Mangefy.Application.Tabs.Queries.GetOpenTabs;

public sealed record OrderItemDto(
    Guid Id,
    Guid MenuItemId,
    string ItemName,
    decimal UnitPrice,
    int Quantity,
    string Status,
    string? Notes
)
{
    public static OrderItemDto FromDomain(OrderItem i) => new(
        i.Id,
        i.MenuItemId,
        i.ItemName,
        i.UnitPrice.Amount,
        i.Quantity,
        i.Status.ToString(),
        i.Notes);
}
