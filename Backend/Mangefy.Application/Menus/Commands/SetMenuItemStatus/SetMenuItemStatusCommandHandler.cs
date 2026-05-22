using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Menus;
using Mangefy.Domain.Menus.Repositories;
using MediatR;

namespace Mangefy.Application.Menus.Commands.SetMenuItemStatus;

public sealed class SetMenuItemStatusCommandHandler : IRequestHandler<SetMenuItemStatusCommand>
{
    private readonly IMenuRepository _menus;
    private readonly IUnitOfWork _uow;

    public SetMenuItemStatusCommandHandler(IMenuRepository menus, IUnitOfWork uow)
    {
        _menus = menus;
        _uow = uow;
    }

    public async Task Handle(SetMenuItemStatusCommand request, CancellationToken cancellationToken)
    {
        var menu = await _menus.GetByIdAsync(request.MenuId, cancellationToken)
            ?? throw new NotFoundException("Cardápio", request.MenuId);

        if (menu.TenantId != request.TenantId)
            throw new ForbiddenException();

        var status = Enum.Parse<MenuItemStatus>(request.Status);
        menu.ChangeItemStatus(request.CategoryId, request.ItemId, status);

        await _menus.UpdateAsync(menu, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
