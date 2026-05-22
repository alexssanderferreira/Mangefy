using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Platform.Features;
using Mangefy.Domain.Tabs;
using Mangefy.Domain.Tabs.Repositories;
using MediatR;

namespace Mangefy.Application.Reports.Queries.GetSalesReport;

public sealed class GetSalesReportQueryHandler : IRequestHandler<GetSalesReportQuery, SalesReportDto>
{
    private readonly ITabRepository _tabs;
    private readonly IFeatureGateService _featureGate;

    public GetSalesReportQueryHandler(ITabRepository tabs, IFeatureGateService featureGate)
    {
        _tabs = tabs;
        _featureGate = featureGate;
    }

    public async Task<SalesReportDto> Handle(GetSalesReportQuery request, CancellationToken cancellationToken)
    {
        await _featureGate.RequireAsync(request.TenantId, FeatureCatalog.Reports.Basic, cancellationToken);

        var closedTabs = await _tabs.GetClosedByPeriodAsync(
            request.TenantId, request.From, request.To, cancellationToken);

        var openTabs = await _tabs.GetOpenByTenantAsync(request.TenantId, cancellationToken);

        var allTabs = closedTabs.ToList();

        var totalRevenue = allTabs.Where(t => t.Status == TabStatus.Closed).Sum(t => t.Total.Amount);
        var totalDiscounts = allTabs.Sum(t => t.DiscountAmount.Amount);
        var totalServiceFees = allTabs.Sum(t => t.ServiceFee.Amount);
        var totalTips = allTabs.Sum(t => t.Tip.Amount);
        var closedCount = allTabs.Count(t => t.Status == TabStatus.Closed);
        var avgTicket = closedCount > 0 ? totalRevenue / closedCount : 0;

        var byDay = allTabs
            .Where(t => t.Status == TabStatus.Closed && t.ClosedAt.HasValue)
            .GroupBy(t => DateOnly.FromDateTime(t.ClosedAt!.Value))
            .Select(g => new DailySaleDto(
                g.Key,
                g.Count(),
                g.Sum(t => t.Total.Amount),
                g.Count() > 0 ? g.Sum(t => t.Total.Amount) / g.Count() : 0))
            .OrderBy(d => d.Date)
            .ToList();

        var topItems = allTabs
            .Where(t => t.Status == TabStatus.Closed)
            .SelectMany(t => t.Orders)
            .Where(o => o.Status != OrderStatus.Cancelled)
            .SelectMany(o => o.Items)
            .Where(i => i.Status != OrderItemStatus.Cancelled)
            .GroupBy(i => new { i.MenuItemId, i.ItemName })
            .Select(g => new TopItemDto(
                g.Key.MenuItemId,
                g.Key.ItemName,
                g.Sum(i => i.Quantity),
                g.Sum(i => i.TotalPrice.Amount)))
            .OrderByDescending(i => i.QuantitySold)
            .Take(20)
            .ToList();

        var cancellations = allTabs
            .Where(t => t.Status == TabStatus.Cancelled && t.ClosedAt.HasValue)
            .Select(t => new CancellationDto(
                t.Id,
                t.Number,
                t.ClosedAt!.Value,
                null,
                t.Subtotal.Amount))
            .ToList();

        var byPayment = allTabs
            .Where(t => t.Status == TabStatus.Closed)
            .SelectMany(t => t.Payments)
            .GroupBy(p => p.Method.ToString())
            .Select(g => new PaymentMethodSummaryDto(g.Key, g.Count(), g.Sum(p => p.Amount.Amount)))
            .ToList();

        // open tabs (pending)
        var openTabCount = openTabs.Count;

        return new SalesReportDto(
            request.From,
            request.To,
            closedCount,
            totalRevenue,
            totalDiscounts,
            totalServiceFees,
            totalTips,
            avgTicket,
            byDay,
            topItems,
            cancellations,
            byPayment);
    }
}
