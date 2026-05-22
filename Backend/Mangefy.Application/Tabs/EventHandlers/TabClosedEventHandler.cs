using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Tables.Repositories;
using Mangefy.Domain.Tabs.Events;
using Mangefy.Domain.Tabs.Repositories;
using MediatR;

namespace Mangefy.Application.Tabs.EventHandlers;

public sealed class TabClosedEventHandler : INotificationHandler<TabClosedEvent>
{
    private readonly ITableRepository _tables;
    private readonly ITabRepository _tabs;

    public TabClosedEventHandler(ITableRepository tables, ITabRepository tabs)
    {
        _tables = tables;
        _tabs = tabs;
    }

    public async Task Handle(TabClosedEvent notification, CancellationToken cancellationToken)
    {
        if (!notification.TableId.HasValue)
            return;

        var openTabs = await _tabs.GetOpenByTableAsync(notification.TenantId, notification.TableId.Value, cancellationToken);
        if (openTabs.Count > 0)
            return; // ainda há comandas abertas na mesa

        var table = await _tables.GetByIdAsync(notification.TableId.Value, cancellationToken);
        if (table is null || table.TenantId != notification.TenantId)
            return;

        table.Release();
        await _tables.UpdateAsync(table, cancellationToken);
    }
}
