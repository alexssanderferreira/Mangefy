using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Menus.Repositories;
using MediatR;

namespace Mangefy.Application.Menus.Commands.DeactivateMenu;

public sealed class DeactivateMenuCommandHandler : IRequestHandler<DeactivateMenuCommand>
{
    private readonly IMenuRepository _menus;
    private readonly IUnitOfWork _uow;

    public DeactivateMenuCommandHandler(IMenuRepository menus, IUnitOfWork uow)
    {
        _menus = menus;
        _uow = uow;
    }

    public async Task Handle(DeactivateMenuCommand request, CancellationToken cancellationToken)
    {
        var menu = await _menus.GetByIdAsync(request.MenuId, cancellationToken)
            ?? throw new NotFoundException("Cardápio", request.MenuId);

        if (menu.TenantId != request.TenantId)
            throw new ForbiddenException();

        menu.Deactivate();
        await _menus.UpdateAsync(menu, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
