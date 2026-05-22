using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Menus;
using Mangefy.Domain.Menus.Repositories;
using Mangefy.Domain.Platform.Features;
using MediatR;

namespace Mangefy.Application.Menus.Commands.CreateMenu;

public sealed class CreateMenuCommandHandler : IRequestHandler<CreateMenuCommand, Guid>
{
    private readonly IMenuRepository _menus;
    private readonly IFeatureGateService _featureGate;
    private readonly IUnitOfWork _uow;

    public CreateMenuCommandHandler(IMenuRepository menus, IFeatureGateService featureGate, IUnitOfWork uow)
    {
        _menus = menus;
        _featureGate = featureGate;
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateMenuCommand request, CancellationToken cancellationToken)
    {
        // Criação de cardápio adicional exige features.multi_menu
        var existingMenus = await _menus.GetAllByTenantAsync(request.TenantId, cancellationToken);
        if (existingMenus.Count > 0)
            await _featureGate.RequireAsync(request.TenantId, FeatureCatalog.Menu.MultiMenu, cancellationToken);

        MenuSchedule? schedule = null;
        if (request.ScheduleDays is not null && request.ScheduleStart.HasValue && request.ScheduleEnd.HasValue)
            schedule = MenuSchedule.Create(request.ScheduleDays, request.ScheduleStart.Value, request.ScheduleEnd.Value);

        var menu = Menu.Create(request.TenantId, request.Name, schedule);
        await _menus.AddAsync(menu, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return menu.Id;
    }
}
