using Mangefy.Domain.Tabs;

namespace Mangefy.Application.Tabs.Queries.GetOpenTabs;

public sealed record OrderDto(
    Guid Id,
    Guid EmployeeId,
    string Status,
    DateTime? SubmittedAt,
    IReadOnlyList<OrderItemDto> Items
)
{
    public static OrderDto FromDomain(Order o) => new(
        o.Id,
        o.EmployeeId,
        o.Status.ToString(),
        o.SubmittedAt,
        o.Items.Select(OrderItemDto.FromDomain).ToList());
}
