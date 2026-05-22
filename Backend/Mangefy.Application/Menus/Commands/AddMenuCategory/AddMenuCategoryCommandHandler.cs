using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Menus;
using Mangefy.Domain.Menus.Repositories;
using MediatR;

namespace Mangefy.Application.Menus.Commands.AddMenuCategory;

public sealed class AddMenuCategoryCommandHandler : IRequestHandler<AddMenuCategoryCommand, Guid>
{
    private readonly IMenuRepository _menus;
    private readonly IUnitOfWork _uow;

    public AddMenuCategoryCommandHandler(IMenuRepository menus, IUnitOfWork uow)
    {
        _menus = menus;
        _uow = uow;
    }

    public async Task<Guid> Handle(AddMenuCategoryCommand request, CancellationToken cancellationToken)
    {
        var menu = await _menus.GetByIdAsync(request.MenuId, cancellationToken)
            ?? throw new NotFoundException(nameof(Menu), request.MenuId);

        if (menu.TenantId != request.TenantId)
            throw new ForbiddenException();

        var category = menu.AddCategory(request.Name, request.DisplayOrder, request.Description);
        await _menus.UpdateAsync(menu, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return category.Id;
    }
}
