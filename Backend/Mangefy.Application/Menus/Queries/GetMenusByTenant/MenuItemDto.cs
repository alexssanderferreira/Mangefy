using Mangefy.Domain.Menus;

namespace Mangefy.Application.Menus.Queries.GetMenusByTenant;

public sealed record RecipeIngredientDto(Guid StockItemId, decimal Quantity);

public sealed record MenuItemDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    string? ImageUrl,
    bool RequiresKds,
    string Station,
    string Status,
    IReadOnlyList<RecipeIngredientDto> Recipe
)
{
    public static MenuItemDto FromDomain(MenuItem i) => new(
        i.Id,
        i.Name,
        i.Description,
        i.Price.Amount,
        i.ImageUrl,
        i.RequiresKds,
        i.Station.ToString(),
        i.Status.ToString(),
        i.Recipe.Select(r => new RecipeIngredientDto(r.StockItemId, r.Quantity)).ToList());
}
