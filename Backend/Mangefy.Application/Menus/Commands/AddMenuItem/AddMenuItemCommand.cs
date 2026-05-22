using Mangefy.Domain.Menus;
using MediatR;

namespace Mangefy.Application.Menus.Commands.AddMenuItem;

public sealed record AddMenuItemCommand(
    Guid TenantId,
    Guid MenuId,
    Guid CategoryId,
    string Name,
    string? Description,
    decimal Price,
    string? ImageUrl,
    bool RequiresKds,
    MenuItemStation Station
) : IRequest<Guid>;
