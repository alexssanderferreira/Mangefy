using MediatR;

namespace Mangefy.Application.Menus.Commands.SetMenuItemRecipe;

public sealed record RecipeIngredientRequest(Guid StockItemId, decimal Quantity);

public sealed record SetMenuItemRecipeCommand(
    Guid TenantId,
    Guid MenuId,
    Guid CategoryId,
    Guid ItemId,
    IReadOnlyList<RecipeIngredientRequest> Ingredients
) : IRequest;
