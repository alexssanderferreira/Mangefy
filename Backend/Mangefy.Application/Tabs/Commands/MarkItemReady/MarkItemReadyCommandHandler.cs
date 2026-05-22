using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Platform.Features;
using Mangefy.Domain.Tabs;
using Mangefy.Domain.Tabs.Repositories;
using MediatR;

namespace Mangefy.Application.Tabs.Commands.MarkItemReady;

public sealed class MarkItemReadyCommandHandler : IRequestHandler<MarkItemReadyCommand>
{
    private readonly ITabRepository _tabs;
    private readonly IUnitOfWork _uow;
    private readonly IFeatureGateService _featureGate;

    public MarkItemReadyCommandHandler(ITabRepository tabs, IUnitOfWork uow, IFeatureGateService featureGate)
    {
        _tabs = tabs;
        _uow = uow;
        _featureGate = featureGate;
    }

    public async Task Handle(MarkItemReadyCommand request, CancellationToken cancellationToken)
    {
        await _featureGate.RequireAsync(request.TenantId, FeatureCatalog.Orders.Kds, cancellationToken);
        var tab = await _tabs.GetByIdAsync(request.TabId, cancellationToken)
            ?? throw new NotFoundException(nameof(Tab), request.TabId);

        if (tab.TenantId != request.TenantId)
            throw new ForbiddenException();

        tab.MarkItemReady(request.OrderId, request.ItemId);
        await _tabs.UpdateAsync(tab, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
