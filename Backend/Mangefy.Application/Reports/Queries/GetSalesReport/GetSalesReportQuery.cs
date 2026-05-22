using MediatR;

namespace Mangefy.Application.Reports.Queries.GetSalesReport;

public sealed record GetSalesReportQuery(
    Guid TenantId,
    DateTime From,
    DateTime To
) : IRequest<SalesReportDto>;

public sealed record SalesReportDto(
    DateTime From,
    DateTime To,
    int TotalTabs,
    decimal TotalRevenue,
    decimal TotalDiscounts,
    decimal TotalServiceFees,
    decimal TotalTips,
    decimal AverageTicket,
    IReadOnlyList<DailySaleDto> ByDay,
    IReadOnlyList<TopItemDto> TopItems,
    IReadOnlyList<CancellationDto> Cancellations,
    IReadOnlyList<PaymentMethodSummaryDto> ByPaymentMethod);

public sealed record DailySaleDto(
    DateOnly Date,
    int TabCount,
    decimal Revenue,
    decimal AverageTicket);

public sealed record TopItemDto(
    Guid MenuItemId,
    string ItemName,
    int QuantitySold,
    decimal TotalRevenue);

public sealed record CancellationDto(
    Guid TabId,
    int TabNumber,
    DateTime CancelledAt,
    string? Reason,
    decimal Amount);

public sealed record PaymentMethodSummaryDto(
    string Method,
    int Count,
    decimal Total);
