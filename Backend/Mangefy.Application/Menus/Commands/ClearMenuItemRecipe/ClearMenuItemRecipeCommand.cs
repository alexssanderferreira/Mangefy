using MediatR;

namespace Mangefy.Application.Menus.Commands.ClearMenuItemRecipe;

public sealed record ClearMenuItemRecipeCommand(
    Guid TenantId,
    Guid MenuId,
    Guid CategoryId,
    Guid ItemId
) : IRequest;
