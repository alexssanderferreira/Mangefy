using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Menus;
using Mangefy.Domain.Menus.Repositories;
using MediatR;

namespace Mangefy.Application.Menus.Commands.UpdateMenuItem;

public sealed class UpdateMenuItemCommandHandler : IRequestHandler<UpdateMenuItemCommand>
{
    private readonly IMenuRepository _menus;
    private readonly IUnitOfWork _uow;

    public UpdateMenuItemCommandHandler(IMenuRepository menus, IUnitOfWork uow)
    {
        _menus = menus;
        _uow = uow;
    }

    public async Task Handle(UpdateMenuItemCommand request, CancellationToken cancellationToken)
    {
        var menu = await _menus.GetByIdAsync(request.MenuId, cancellationToken)
            ?? throw new NotFoundException("Cardápio", request.MenuId);

        if (menu.TenantId != request.TenantId)
            throw new ForbiddenException();

        var station = Enum.Parse<MenuItemStation>(request.Station);
        menu.UpdateItem(request.CategoryId, request.ItemId, request.Name, request.Description,
            request.Price, request.ImageUrl, request.RequiresKds, station);

        await _menus.UpdateAsync(menu, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
