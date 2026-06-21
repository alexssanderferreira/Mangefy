using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Menus;
using Mangefy.Domain.Menus.Repositories;
using Mangefy.Domain.Plans;
using Mangefy.Domain.Plans.Repositories;
using Mangefy.Domain.Tenants;
using Mangefy.Domain.Tenants.Repositories;
using MediatR;

namespace Mangefy.Application.Menus.Commands.AddMenuItem;

public sealed class AddMenuItemCommandHandler : IRequestHandler<AddMenuItemCommand, Guid>
{
    private readonly IMenuRepository _menus;
    private readonly ITenantRepository _tenants;
    private readonly IPlanRepository _plans;
    private readonly IUnitOfWork _uow;

    public AddMenuItemCommandHandler(
        IMenuRepository menus,
        ITenantRepository tenants,
        IPlanRepository plans,
        IUnitOfWork uow)
    {
        _menus = menus;
        _tenants = tenants;
        _plans = plans;
        _uow = uow;
    }

    public async Task<Guid> Handle(AddMenuItemCommand request, CancellationToken cancellationToken)
    {
        var menu = await _menus.GetByIdAsync(request.MenuId, cancellationToken)
            ?? throw new NotFoundException(nameof(Menu), request.MenuId);

        if (menu.TenantId != request.TenantId)
            throw new ForbiddenException();

        var tenant = await _tenants.GetByIdAsync(request.TenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenant), request.TenantId);

        var plan = await _plans.GetByIdAsync(tenant.PlanId, cancellationToken)
            ?? throw new NotFoundException(nameof(Plan), tenant.PlanId);

        var currentCount = await _menus.CountItemsByTenantAsync(request.TenantId, cancellationToken);
        if (currentCount >= plan.MaxMenuItems)
            throw new ConflictException($"Limite de {plan.MaxMenuItems} item(ns) de cardápio atingido pelo plano atual.");

        var item = menu.AddItemToCategory(
            request.CategoryId, request.Name, request.Description,
            request.Price, request.ImageUrl, request.RequiresKds);

        await _menus.UpdateAsync(menu, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return item.Id;
    }
}
