using Mangefy.Domain.Menus.Repositories;
using MediatR;

namespace Mangefy.Application.Menus.Queries.GetMenusByTenant;

public sealed class GetMenusByTenantQueryHandler
    : IRequestHandler<GetMenusByTenantQuery, IReadOnlyList<MenuDto>>
{
    private readonly IMenuRepository _menus;

    public GetMenusByTenantQueryHandler(IMenuRepository menus)
        => _menus = menus;

    public async Task<IReadOnlyList<MenuDto>> Handle(
        GetMenusByTenantQuery request, CancellationToken cancellationToken)
    {
        var menus = await _menus.GetAllByTenantAsync(request.TenantId, cancellationToken);
        return menus.Select(MenuDto.FromDomain).ToList();
    }
}
