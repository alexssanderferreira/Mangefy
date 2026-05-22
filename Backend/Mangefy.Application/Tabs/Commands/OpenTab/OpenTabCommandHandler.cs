using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Common;
using Mangefy.Domain.Settings.Repositories;
using Mangefy.Domain.Tables;
using Mangefy.Domain.Tables.Repositories;
using Mangefy.Domain.Tabs;
using Mangefy.Domain.Tabs.Repositories;
using MediatR;

namespace Mangefy.Application.Tabs.Commands.OpenTab;

public sealed class OpenTabCommandHandler : IRequestHandler<OpenTabCommand, Guid>
{
    private readonly ITabRepository _tabs;
    private readonly ITableRepository _tables;
    private readonly ITabSettingsRepository _tabSettings;
    private readonly IUnitOfWork _uow;

    public OpenTabCommandHandler(
        ITabRepository tabs,
        ITableRepository tables,
        ITabSettingsRepository tabSettings,
        IUnitOfWork uow)
    {
        _tabs = tabs;
        _tables = tables;
        _tabSettings = tabSettings;
        _uow = uow;
    }

    public async Task<Guid> Handle(OpenTabCommand request, CancellationToken cancellationToken)
    {
        if (request.TableId.HasValue)
        {
            var table = await _tables.GetByIdAsync(request.TableId.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(Table), request.TableId.Value);

            if (table.TenantId != request.TenantId)
                throw new ForbiddenException();
        }

        // Numeração respeita TabSettings (min/max, reutiliza números fechados)
        var settings = await _tabSettings.GetByTenantIdAsync(request.TenantId, cancellationToken);
        int min = settings?.MinTabNumber ?? 1;
        int max = settings?.MaxTabNumber ?? 9999;

        var number = await _tabs.GetNextAvailableNumberAsync(request.TenantId, min, max, cancellationToken);
        if (number is null)
            throw new DomainException($"Não há números de comanda disponíveis no intervalo {min}–{max}. Feche ou cancele comandas existentes.");

        var tab = Tab.Open(
            request.TenantId, number.Value, request.CustomerName,
            request.EmployeeId, request.TableId, request.LocationNote,
            request.Channel, request.DeliveryInfo);

        await _tabs.AddAsync(tab, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return tab.Id;
    }
}
