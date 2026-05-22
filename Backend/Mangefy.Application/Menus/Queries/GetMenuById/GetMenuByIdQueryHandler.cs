using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Menus.Queries.GetMenusByTenant;
using Mangefy.Domain.Menus.Repositories;
using MediatR;

namespace Mangefy.Application.Menus.Queries.GetMenuById;

public sealed class GetMenuByIdQueryHandler
    : IRequestHandler<GetMenuByIdQuery, MenuDto>
{
    private readonly IMenuRepository _menus;

    public GetMenuByIdQueryHandler(IMenuRepository menus)
        => _menus = menus;

    public async Task<MenuDto> Handle(
        GetMenuByIdQuery request, CancellationToken cancellationToken)
    {
        var menu = await _menus.GetByIdAsync(request.MenuId, cancellationToken)
            ?? throw new NotFoundException("Cardápio", request.MenuId);

        if (menu.TenantId != request.TenantId)
            throw new ForbiddenException();

        return MenuDto.FromDomain(menu);
    }
}
