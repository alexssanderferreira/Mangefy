using Mangefy.Application.Menus.Queries.GetMenusByTenant;
using MediatR;

namespace Mangefy.Application.Menus.Queries.GetMenuById;

public sealed record GetMenuByIdQuery(Guid TenantId, Guid MenuId)
    : IRequest<MenuDto>;
