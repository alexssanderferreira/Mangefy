using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Platform.Features;
using Mangefy.Domain.Stock.Repositories;
using Mangefy.Domain.Tabs;
using Mangefy.Domain.Tabs.Repositories;
using MediatR;

namespace Mangefy.Application.Reports.Queries.GetOperationalReport;

public sealed class GetOperationalReportQueryHandler : IRequestHandler<GetOperationalReportQuery, OperationalReportDto>
{
    private readonly ITabRepository _tabs;
    private readonly IStockRepository _stock;
    private readonly IFeatureGateService _featureGate;

    public GetOperationalReportQueryHandler(
        ITabRepository tabs,
        IStockRepository stock,
        IFeatureGateService featureGate)
    {
        _tabs = tabs;
        _stock = stock;
        _featureGate = featureGate;
    }

    public async Task<OperationalReportDto> Handle(GetOperationalReportQuery request, CancellationToken cancellationToken)
    {
        await _featureGate.RequireAsync(request.TenantId, FeatureCatalog.Reports.Basic, cancellationToken);

        var openTabs = await _tabs.GetOpenByTenantAsync(request.TenantId, cancellationToken);
        var stock = await _stock.GetByTenantIdAsync(request.TenantId, cancellationToken);

        var openTabSummaries = openTabs
            .Select(t => new OpenTabSummaryDto(
                t.Id,
                t.Number,
                t.CustomerName,
                t.OpenedAt,
                t.Total.Amount,
                t.Orders.Count))
            .OrderBy(t => t.OpenedAt)
            .ToList();

        var now = DateTime.UtcNow;
        var delayed = openTabs
            .SelectMany(t => t.Orders
                .SelectMany(o => o.Items
                    .Where(i => i.Status == OrderItemStatus.Sent && i.SentToKitchenAt.HasValue)
                    .Select(i => new DelayedOrderDto(
                        t.Id,
                        t.Number,
                        o.Id,
                        i.Id,
                        i.ItemName,
                        i.SentToKitchenAt!.Value,
                        (now - i.SentToKitchenAt.Value).TotalMinutes))))
            .Where(d => d.MinutesWaiting > 15)
            .OrderByDescending(d => d.MinutesWaiting)
            .ToList();

        var lowStock = stock?.GetItemsBelowMinimum()
            .Select(i => new LowStockItemDto(
                i.Id,
                i.Name,
                i.CurrentQuantity,
                i.MinimumQuantity,
                i.Unit.ToString()))
            .ToList() ?? new List<LowStockItemDto>();

        return new OperationalReportDto(openTabs.Count, openTabSummaries, delayed, lowStock);
    }
}
