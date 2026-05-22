using Mangefy.API.Filters;
using Mangefy.Application.Platform.SupplierCategories.Commands.CreateGlobalSupplierCategory;
using Mangefy.Application.Platform.SupplierCategories.Commands.DeleteSupplierCategory;
using Mangefy.Application.Platform.SupplierCategories.Commands.ToggleSupplierCategory;
using Mangefy.Application.Platform.SupplierCategories.Commands.UpdateSupplierCategory;
using Mangefy.Application.Platform.SupplierCategories.Queries.GetGlobalSupplierCategories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers.Admin;

[ApiController]
[Route("api/admin/supplier-categories")]
[Authorize]
[RequireAdminSaas]
public sealed class SupplierCategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public SupplierCategoriesController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _sender.Send(new GetGlobalSupplierCategoriesQuery(), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplierCategoryRequest request, CancellationToken ct)
    {
        var id = await _sender.Send(new CreateGlobalSupplierCategoryCommand(request.Name, request.Description), ct);
        return Created($"/api/admin/supplier-categories/{id}", new { Id = id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierCategoryRequest request, CancellationToken ct)
    {
        await _sender.Send(new UpdateSupplierCategoryCommand(id, request.Name, request.Description), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await _sender.Send(new ToggleSupplierCategoryCommand(id, Activate: true), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _sender.Send(new ToggleSupplierCategoryCommand(id, Activate: false), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteSupplierCategoryCommand(id), ct);
        return NoContent();
    }
}

public sealed record CreateSupplierCategoryRequest(string Name, string? Description = null);
public sealed record UpdateSupplierCategoryRequest(string Name, string? Description);
