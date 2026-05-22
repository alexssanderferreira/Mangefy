using MediatR;

namespace Mangefy.Application.Menus.Commands.AddMenuCategory;

public sealed record AddMenuCategoryCommand(
    Guid TenantId,
    Guid MenuId,
    string Name,
    string? Description,
    int DisplayOrder
) : IRequest<Guid>;
