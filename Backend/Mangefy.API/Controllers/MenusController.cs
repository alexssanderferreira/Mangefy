using Mangefy.API.Filters;
using Mangefy.Application.Menus.Commands.ClearMenuItemRecipe;
using Mangefy.Application.Menus.Commands.SetMenuItemRecipe;
using Mangefy.Application.Menus.Commands.ActivateMenu;
using Mangefy.Application.Menus.Commands.AddMenuCategory;
using Mangefy.Application.Menus.Commands.AddMenuItem;
using Mangefy.Application.Menus.Commands.CreateMenu;
using Mangefy.Application.Menus.Commands.DeactivateMenu;
using Mangefy.Application.Menus.Commands.RemoveMenuCategory;
using Mangefy.Application.Menus.Commands.RemoveMenuItem;
using Mangefy.Application.Menus.Commands.SetMenuItemStatus;
using Mangefy.Application.Menus.Commands.UpdateMenuCategory;
using Mangefy.Application.Menus.Commands.UpdateMenuItem;
using Mangefy.Application.Menus.Queries.GetMenuById;
using Mangefy.Application.Menus.Queries.GetMenusByTenant;
using Mangefy.Domain.Menus;
using Mangefy.Domain.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/[controller]")]
[Authorize]
[ValidateTenantAccess]
public sealed class MenusController : ControllerBase
{
    private readonly ISender _sender;
    public MenusController(ISender sender) => _sender = sender;

    [HttpGet]
    [RequirePermission(PermissionCatalog.Menu.Read)]
    public async Task<IActionResult> GetAll(Guid tenantId, CancellationToken ct)
        => Ok(await _sender.Send(new GetMenusByTenantQuery(tenantId), ct));

    [HttpGet("{menuId:guid}")]
    [RequirePermission(PermissionCatalog.Menu.Read)]
    public async Task<IActionResult> GetById(Guid tenantId, Guid menuId, CancellationToken ct)
        => Ok(await _sender.Send(new GetMenuByIdQuery(tenantId, menuId), ct));

    [HttpPost]
    [RequirePermission(PermissionCatalog.Menu.Manage)]
    public async Task<IActionResult> Create(Guid tenantId, [FromBody] CreateMenuRequest request, CancellationToken ct)
    {
        var id = await _sender.Send(new CreateMenuCommand(
            tenantId, request.Name, request.ScheduleDays, request.ScheduleStart, request.ScheduleEnd), ct);
        return Created($"/api/tenants/{tenantId}/menus/{id}", new { id });
    }

    [HttpPatch("{menuId:guid}/activate")]
    [RequirePermission(PermissionCatalog.Menu.Manage)]
    public async Task<IActionResult> Activate(Guid tenantId, Guid menuId, CancellationToken ct)
    {
        await _sender.Send(new ActivateMenuCommand(tenantId, menuId), ct);
        return NoContent();
    }

    [HttpPatch("{menuId:guid}/deactivate")]
    [RequirePermission(PermissionCatalog.Menu.Manage)]
    public async Task<IActionResult> Deactivate(Guid tenantId, Guid menuId, CancellationToken ct)
    {
        await _sender.Send(new DeactivateMenuCommand(tenantId, menuId), ct);
        return NoContent();
    }

    [HttpPost("{menuId:guid}/categories")]
    [RequirePermission(PermissionCatalog.Menu.Manage)]
    public async Task<IActionResult> AddCategory(Guid tenantId, Guid menuId, [FromBody] AddCategoryRequest request, CancellationToken ct)
    {
        var id = await _sender.Send(new AddMenuCategoryCommand(
            tenantId, menuId, request.Name, request.Description, request.DisplayOrder), ct);
        return Created(string.Empty, new { id });
    }

    [HttpPut("{menuId:guid}/categories/{categoryId:guid}")]
    [RequirePermission(PermissionCatalog.Menu.Manage)]
    public async Task<IActionResult> UpdateCategory(Guid tenantId, Guid menuId, Guid categoryId, [FromBody] UpdateCategoryRequest request, CancellationToken ct)
    {
        await _sender.Send(new UpdateMenuCategoryCommand(
            tenantId, menuId, categoryId, request.Name, request.Description, request.DisplayOrder), ct);
        return NoContent();
    }

    [HttpDelete("{menuId:guid}/categories/{categoryId:guid}")]
    [RequirePermission(PermissionCatalog.Menu.Manage)]
    public async Task<IActionResult> RemoveCategory(Guid tenantId, Guid menuId, Guid categoryId, CancellationToken ct)
    {
        await _sender.Send(new RemoveMenuCategoryCommand(tenantId, menuId, categoryId), ct);
        return NoContent();
    }

    [HttpPost("{menuId:guid}/categories/{categoryId:guid}/items")]
    [RequirePermission(PermissionCatalog.Menu.Manage)]
    public async Task<IActionResult> AddItem(Guid tenantId, Guid menuId, Guid categoryId, [FromBody] AddItemRequest request, CancellationToken ct)
    {
        var id = await _sender.Send(new AddMenuItemCommand(
            tenantId, menuId, categoryId, request.Name, request.Description,
            request.Price, request.ImageUrl, request.RequiresKds, request.Station), ct);
        return Created(string.Empty, new { id });
    }

    [HttpPut("{menuId:guid}/categories/{categoryId:guid}/items/{itemId:guid}")]
    [RequirePermission(PermissionCatalog.Menu.Manage)]
    public async Task<IActionResult> UpdateItem(Guid tenantId, Guid menuId, Guid categoryId, Guid itemId, [FromBody] UpdateItemRequest request, CancellationToken ct)
    {
        await _sender.Send(new UpdateMenuItemCommand(
            tenantId, menuId, categoryId, itemId,
            request.Name, request.Description, request.Price, request.ImageUrl, request.RequiresKds, request.Station), ct);
        return NoContent();
    }

    [HttpDelete("{menuId:guid}/categories/{categoryId:guid}/items/{itemId:guid}")]
    [RequirePermission(PermissionCatalog.Menu.Manage)]
    public async Task<IActionResult> RemoveItem(Guid tenantId, Guid menuId, Guid categoryId, Guid itemId, CancellationToken ct)
    {
        await _sender.Send(new RemoveMenuItemCommand(tenantId, menuId, categoryId, itemId), ct);
        return NoContent();
    }

    [HttpPatch("{menuId:guid}/categories/{categoryId:guid}/items/{itemId:guid}/status")]
    [RequirePermission(PermissionCatalog.Menu.Manage)]
    public async Task<IActionResult> SetItemStatus(Guid tenantId, Guid menuId, Guid categoryId, Guid itemId, [FromBody] SetItemStatusRequest request, CancellationToken ct)
    {
        await _sender.Send(new SetMenuItemStatusCommand(tenantId, menuId, categoryId, itemId, request.Status), ct);
        return NoContent();
    }

    [HttpPut("{menuId:guid}/categories/{categoryId:guid}/items/{itemId:guid}/recipe")]
    [RequirePermission(PermissionCatalog.Menu.Manage)]
    public async Task<IActionResult> SetRecipe(Guid tenantId, Guid menuId, Guid categoryId, Guid itemId,
        [FromBody] SetRecipeRequest request, CancellationToken ct)
    {
        var ingredients = request.Ingredients.Select(i => new RecipeIngredientRequest(i.StockItemId, i.Quantity)).ToList();
        await _sender.Send(new SetMenuItemRecipeCommand(tenantId, menuId, categoryId, itemId, ingredients), ct);
        return NoContent();
    }

    [HttpDelete("{menuId:guid}/categories/{categoryId:guid}/items/{itemId:guid}/recipe")]
    [RequirePermission(PermissionCatalog.Menu.Manage)]
    public async Task<IActionResult> ClearRecipe(Guid tenantId, Guid menuId, Guid categoryId, Guid itemId, CancellationToken ct)
    {
        await _sender.Send(new ClearMenuItemRecipeCommand(tenantId, menuId, categoryId, itemId), ct);
        return NoContent();
    }
}

public sealed record CreateMenuRequest(string Name, IReadOnlyList<DayOfWeek>? ScheduleDays, TimeOnly? ScheduleStart, TimeOnly? ScheduleEnd);
public sealed record AddCategoryRequest(string Name, string? Description, int DisplayOrder);
public sealed record UpdateCategoryRequest(string Name, string? Description, int DisplayOrder);
public sealed record AddItemRequest(string Name, string? Description, decimal Price, string? ImageUrl, bool RequiresKds, MenuItemStation Station);
public sealed record UpdateItemRequest(string Name, string? Description, decimal Price, string? ImageUrl, bool RequiresKds, string Station);
public sealed record SetItemStatusRequest(string Status);
public sealed record RecipeIngredientApiRequest(Guid StockItemId, decimal Quantity);
public sealed record SetRecipeRequest(IReadOnlyList<RecipeIngredientApiRequest> Ingredients);
