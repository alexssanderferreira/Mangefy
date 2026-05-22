using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Menus.Repositories;
using Mangefy.Domain.Stock.Repositories;
using Mangefy.Domain.Tabs.Events;
using Mangefy.Domain.Tabs.Repositories;
using MediatR;

namespace Mangefy.Application.Tabs.EventHandlers;

/// <summary>
/// Deduz automaticamente o estoque ao marcar um pedido como pronto.
/// Cada item do pedido que possui ficha técnica no cardápio gera uma baixa.
/// </summary>
public sealed class OrderReadyEventHandler : INotificationHandler<OrderReadyEvent>
{
    private readonly ITabRepository _tabs;
    private readonly IMenuRepository _menus;
    private readonly IStockRepository _stock;
    private readonly IUnitOfWork _uow;

    public OrderReadyEventHandler(
        ITabRepository tabs,
        IMenuRepository menus,
        IStockRepository stock,
        IUnitOfWork uow)
    {
        _tabs = tabs;
        _menus = menus;
        _stock = stock;
        _uow = uow;
    }

    public async Task Handle(OrderReadyEvent notification, CancellationToken cancellationToken)
    {
        var tab = await _tabs.GetByIdAsync(notification.TabId, cancellationToken);
        if (tab is null) return;

        var order = tab.Orders.FirstOrDefault(o => o.Id == notification.OrderId);
        if (order is null) return;

        var stock = await _stock.GetByTenantIdAsync(notification.TenantId, cancellationToken);
        if (stock is null) return;

        var menus = await _menus.GetAllByTenantAsync(notification.TenantId, cancellationToken);

        foreach (var item in order.Items.Where(i => i.Status == Domain.Tabs.OrderItemStatus.Ready))
        {
            var menuItem = menus
                .SelectMany(m => m.Categories)
                .SelectMany(c => c.Items)
                .FirstOrDefault(i => i.Id == item.MenuItemId);

            if (menuItem is null || !menuItem.HasRecipe()) continue;

            foreach (var ingredient in menuItem.Recipe)
            {
                var stockItem = stock.Items.FirstOrDefault(s => s.Id == ingredient.StockItemId);
                if (stockItem is null || stockItem.CurrentQuantity < ingredient.Quantity * item.Quantity)
                    continue;

                stock.DeductForSale(ingredient.StockItemId, ingredient.Quantity * item.Quantity, item.Id);
            }
        }

        await _stock.UpdateAsync(stock, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
