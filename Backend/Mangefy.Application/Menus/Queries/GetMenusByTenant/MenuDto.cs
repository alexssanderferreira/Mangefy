using Mangefy.Domain.Menus;

namespace Mangefy.Application.Menus.Queries.GetMenusByTenant;

public sealed record MenuDto(
    Guid Id,
    string Name,
    bool IsActive,
    bool IsDefault,
    IReadOnlyList<MenuCategoryDto> Categories
)
{
    public static MenuDto FromDomain(Menu m) => new(
        m.Id,
        m.Name,
        m.IsActive,
        m.IsDefault,
        m.Categories.Select(MenuCategoryDto.FromDomain).ToList());
}
