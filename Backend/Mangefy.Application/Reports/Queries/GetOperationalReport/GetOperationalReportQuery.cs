using MediatR;

namespace Mangefy.Application.Reports.Queries.GetOperationalReport;

public sealed record GetOperationalReportQuery(
    Guid TenantId
) : IRequest<OperationalReportDto>;

public sealed record OperationalReportDto(
    int OpenTabsCount,
    IReadOnlyList<OpenTabSummaryDto> OpenTabs,
    IReadOnlyList<DelayedOrderDto> DelayedOrders,
    IReadOnlyList<LowStockItemDto> LowStockItems);

public sealed record OpenTabSummaryDto(
    Guid TabId,
    int TabNumber,
    string CustomerName,
    DateTime OpenedAt,
    decimal CurrentTotal,
    int OrderCount);

public sealed record DelayedOrderDto(
    Guid TabId,
    int TabNumber,
    Guid OrderId,
    Guid ItemId,
    string ItemName,
    DateTime SentToKitchenAt,
    double MinutesWaiting);

public sealed record LowStockItemDto(
    Guid StockItemId,
    string ItemName,
    decimal CurrentQuantity,
    decimal MinimumQuantity,
    string Unit);
