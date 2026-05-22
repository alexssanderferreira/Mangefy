using Mangefy.Domain.Tabs;

namespace Mangefy.Application.Tabs.Queries.GetOpenTabs;

public sealed record TabDto(
    Guid Id,
    int Number,
    string CustomerName,
    Guid? CurrentTableId,
    string? LocationNote,
    string Status,
    decimal Total,
    decimal TotalPaid,
    DateTime OpenedAt,
    IReadOnlyList<OrderDto> Orders
)
{
    public static TabDto FromDomain(Tab t) => new(
        t.Id,
        t.Number,
        t.CustomerName,
        t.CurrentTableId,
        t.LocationNote,
        t.Status.ToString(),
        t.Total.Amount,
        t.TotalPaid.Amount,
        t.OpenedAt,
        t.Orders.Select(OrderDto.FromDomain).ToList());
}
