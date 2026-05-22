using MediatR;

namespace Mangefy.Application.Menus.Queries.GetMenusByTenant;

public sealed record GetMenusByTenantQuery(Guid TenantId)
    : IRequest<IReadOnlyList<MenuDto>>;
