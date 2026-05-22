using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Common;
using Mangefy.Domain.Menus;
using Mangefy.Domain.Menus.Repositories;
using Mangefy.Domain.Tabs;
using Mangefy.Domain.Tabs.Repositories;
using MediatR;

namespace Mangefy.Application.Tabs.Commands.SubmitOrder;

public sealed class SubmitOrderCommandHandler : IRequestHandler<SubmitOrderCommand, Guid>
{
    private readonly ITabRepository _tabs;
    private readonly IMenuRepository _menus;
    private readonly IUnitOfWork _uow;

    public SubmitOrderCommandHandler(ITabRepository tabs, IMenuRepository menus, IUnitOfWork uow)
    {
        _tabs = tabs;
        _menus = menus;
        _uow = uow;
    }

    public async Task<Guid> Handle(SubmitOrderCommand request, CancellationToken cancellationToken)
    {
        var tab = await _tabs.GetByIdAsync(request.TabId, cancellationToken)
            ?? throw new NotFoundException(nameof(Tab), request.TabId);

        if (tab.TenantId != request.TenantId)
            throw new ForbiddenException();

        var order = tab.AddOrder(request.EmployeeId);

        var menuItemIds = request.Items.Select(i => i.MenuItemId).Distinct();
        var menuItems = await _menus.GetItemsByIdsAsync(menuItemIds, cancellationToken);
        var menuItemMap = menuItems.ToDictionary(m => m.Id);

        foreach (var item in request.Items)
        {
            if (!menuItemMap.TryGetValue(item.MenuItemId, out var menuItem))
                throw new NotFoundException(nameof(MenuItem), item.MenuItemId);

            if (menuItem.TenantId != request.TenantId)
                throw new ForbiddenException();

            if (menuItem.Status != MenuItemStatus.Available)
                throw new DomainException($"Item '{menuItem.Name}' não está disponível no momento.");

            tab.AddItemToOrder(
                order.Id,
                menuItem.Id,
                menuItem.Name,
                menuItem.GetEffectivePrice(),
                item.Quantity,
                menuItem.RequiresKds,
                station: menuItem.Station,
                notes: item.Notes,
                modifiers: item.Modifiers);
        }

        tab.SubmitOrder(order.Id);

        await _tabs.UpdateAsync(tab, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return order.Id;
    }
}
