using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Menus;
using Mangefy.Domain.Menus.Repositories;
using MediatR;

namespace Mangefy.Application.Menus.Commands.AddMenuItem;

public sealed class AddMenuItemCommandHandler : IRequestHandler<AddMenuItemCommand, Guid>
{
    private readonly IMenuRepository _menus;
    private readonly IUnitOfWork _uow;

    public AddMenuItemCommandHandler(IMenuRepository menus, IUnitOfWork uow)
    {
        _menus = menus;
        _uow = uow;
    }

    public async Task<Guid> Handle(AddMenuItemCommand request, CancellationToken cancellationToken)
    {
        var menu = await _menus.GetByIdAsync(request.MenuId, cancellationToken)
            ?? throw new NotFoundException(nameof(Menu), request.MenuId);

        if (menu.TenantId != request.TenantId)
            throw new ForbiddenException();

        var item = menu.AddItemToCategory(
            request.CategoryId, request.Name, request.Description,
            request.Price, request.ImageUrl, request.RequiresKds);

        await _menus.UpdateAsync(menu, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return item.Id;
    }
}
