using Mangefy.Domain.Menus;

namespace Mangefy.Application.Menus.Queries.GetMenusByTenant;

public sealed record MenuCategoryDto(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyList<MenuItemDto> Items
)
{
    public static MenuCategoryDto FromDomain(MenuCategory c) => new(
        c.Id,
        c.Name,
        c.Description,
        c.DisplayOrder,
        c.IsActive,
        c.Items.Select(MenuItemDto.FromDomain).ToList());
}
