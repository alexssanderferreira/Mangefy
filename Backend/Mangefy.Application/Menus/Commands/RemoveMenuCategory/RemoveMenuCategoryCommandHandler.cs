using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Menus.Repositories;
using MediatR;

namespace Mangefy.Application.Menus.Commands.RemoveMenuCategory;

public sealed class RemoveMenuCategoryCommandHandler : IRequestHandler<RemoveMenuCategoryCommand>
{
    private readonly IMenuRepository _menus;
    private readonly IUnitOfWork _uow;

    public RemoveMenuCategoryCommandHandler(IMenuRepository menus, IUnitOfWork uow)
    {
        _menus = menus;
        _uow = uow;
    }

    public async Task Handle(RemoveMenuCategoryCommand request, CancellationToken cancellationToken)
    {
        var menu = await _menus.GetByIdAsync(request.MenuId, cancellationToken)
            ?? throw new NotFoundException("Cardápio", request.MenuId);

        if (menu.TenantId != request.TenantId)
            throw new ForbiddenException();

        menu.RemoveCategory(request.CategoryId);
        await _menus.UpdateAsync(menu, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
