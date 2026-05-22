using MediatR;

namespace Mangefy.Application.Menus.Commands.UpdateMenuCategory;

public sealed record UpdateMenuCategoryCommand(
    Guid TenantId,
    Guid MenuId,
    Guid CategoryId,
    string Name,
    string? Description,
    int DisplayOrder) : IRequest;
