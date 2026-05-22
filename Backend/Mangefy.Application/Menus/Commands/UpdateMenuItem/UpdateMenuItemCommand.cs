using MediatR;

namespace Mangefy.Application.Menus.Commands.UpdateMenuItem;

public sealed record UpdateMenuItemCommand(
    Guid TenantId,
    Guid MenuId,
    Guid CategoryId,
    Guid ItemId,
    string Name,
    string? Description,
    decimal Price,
    string? ImageUrl,
    bool RequiresKds,
    string Station) : IRequest;
