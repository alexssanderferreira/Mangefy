using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Menus.Repositories;
using MediatR;

namespace Mangefy.Application.Menus.Commands.RemoveMenuItem;

public sealed class RemoveMenuItemCommandHandler : IRequestHandler<RemoveMenuItemCommand>
{
    private readonly IMenuRepository _menus;
    private readonly IUnitOfWork _uow;

    public RemoveMenuItemCommandHandler(IMenuRepository menus, IUnitOfWork uow)
    {
        _menus = menus;
        _uow = uow;
    }

    public async Task Handle(RemoveMenuItemCommand request, CancellationToken cancellationToken)
    {
        var menu = await _menus.GetByIdAsync(request.MenuId, cancellationToken)
            ?? throw new NotFoundException("Cardápio", request.MenuId);

        if (menu.TenantId != request.TenantId)
            throw new ForbiddenException();

        menu.RemoveItemFromCategory(request.CategoryId, request.ItemId);
        await _menus.UpdateAsync(menu, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
