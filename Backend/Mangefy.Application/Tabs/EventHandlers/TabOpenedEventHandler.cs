using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Tables.Repositories;
using Mangefy.Domain.Tabs.Events;
using MediatR;

namespace Mangefy.Application.Tabs.EventHandlers;

public sealed class TabOpenedEventHandler : INotificationHandler<TabOpenedEvent>
{
    private readonly ITableRepository _tables;
    private readonly IUnitOfWork _uow;

    public TabOpenedEventHandler(ITableRepository tables, IUnitOfWork uow)
    {
        _tables = tables;
        _uow = uow;
    }

    public async Task Handle(TabOpenedEvent notification, CancellationToken cancellationToken)
    {
        if (!notification.TableId.HasValue)
            return;

        var table = await _tables.GetByIdAsync(notification.TableId.Value, cancellationToken);
        if (table is null || table.TenantId != notification.TenantId)
            return;

        table.Occupy();
        await _tables.UpdateAsync(table, cancellationToken);
        // UnitOfWork faz segundo SaveChangesAsync após publicar todos os domain events
    }
}
