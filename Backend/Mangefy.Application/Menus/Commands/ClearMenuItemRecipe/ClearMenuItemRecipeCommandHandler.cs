using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Menus;
using Mangefy.Domain.Menus.Repositories;
using MediatR;

namespace Mangefy.Application.Menus.Commands.ClearMenuItemRecipe;

public sealed class ClearMenuItemRecipeCommandHandler : IRequestHandler<ClearMenuItemRecipeCommand>
{
    private readonly IMenuRepository _menus;
    private readonly IUnitOfWork _uow;

    public ClearMenuItemRecipeCommandHandler(IMenuRepository menus, IUnitOfWork uow)
    {
        _menus = menus;
        _uow = uow;
    }

    public async Task Handle(ClearMenuItemRecipeCommand request, CancellationToken cancellationToken)
    {
        var menu = await _menus.GetByIdAsync(request.MenuId, cancellationToken)
            ?? throw new NotFoundException(nameof(Menu), request.MenuId);

        if (menu.TenantId != request.TenantId)
            throw new ForbiddenException();

        menu.ClearItemRecipe(request.CategoryId, request.ItemId);

        await _menus.UpdateAsync(menu, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
