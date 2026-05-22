using Mangefy.API.Filters;
using Mangefy.Application.Platform.BusinessTypes.Commands.AddRoleTemplate;
using Mangefy.Application.Platform.BusinessTypes.Commands.CreateBusinessType;
using Mangefy.Application.Platform.BusinessTypes.Commands.DeleteBusinessType;
using Mangefy.Application.Platform.BusinessTypes.Commands.DeleteRoleTemplate;
using Mangefy.Application.Platform.BusinessTypes.Commands.ToggleBusinessType;
using Mangefy.Application.Platform.BusinessTypes.Commands.ToggleRoleTemplate;
using Mangefy.Application.Platform.BusinessTypes.Commands.UpdateBusinessType;
using Mangefy.Application.Platform.BusinessTypes.Commands.UpdateRoleTemplate;
using Mangefy.Application.Platform.BusinessTypes.Queries.GetBusinessTypes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers.Admin;

[ApiController]
[Route("api/admin/business-types")]
[Authorize]
[RequireAdminSaas]
public sealed class BusinessTypesController : ControllerBase
{
    private readonly ISender _sender;

    public BusinessTypesController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _sender.Send(new GetBusinessTypesQuery(), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBusinessTypeRequest request, CancellationToken ct)
    {
        var id = await _sender.Send(new CreateBusinessTypeCommand(request.Name, request.Description), ct);
        return Created($"/api/admin/business-types/{id}", new { Id = id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBusinessTypeRequest request, CancellationToken ct)
    {
        await _sender.Send(new UpdateBusinessTypeCommand(id, request.Name, request.Description), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await _sender.Send(new ToggleBusinessTypeCommand(id, Activate: true), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _sender.Send(new ToggleBusinessTypeCommand(id, Activate: false), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteBusinessTypeCommand(id), ct);
        return NoContent();
    }

    // ── Role Templates ──────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/role-templates")]
    public async Task<IActionResult> AddTemplate(Guid id, [FromBody] RoleTemplateRequest request, CancellationToken ct)
    {
        var templateId = await _sender.Send(new AddRoleTemplateCommand(id, request.Name, request.Description, request.Permissions), ct);
        return Created(string.Empty, new { Id = templateId });
    }

    [HttpPut("{id:guid}/role-templates/{templateId:guid}")]
    public async Task<IActionResult> UpdateTemplate(Guid id, Guid templateId, [FromBody] RoleTemplateRequest request, CancellationToken ct)
    {
        await _sender.Send(new UpdateRoleTemplateCommand(id, templateId, request.Name, request.Description, request.Permissions), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/role-templates/{templateId:guid}/activate")]
    public async Task<IActionResult> ActivateTemplate(Guid id, Guid templateId, CancellationToken ct)
    {
        await _sender.Send(new ToggleRoleTemplateCommand(id, templateId, Activate: true), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/role-templates/{templateId:guid}/deactivate")]
    public async Task<IActionResult> DeactivateTemplate(Guid id, Guid templateId, CancellationToken ct)
    {
        await _sender.Send(new ToggleRoleTemplateCommand(id, templateId, Activate: false), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/role-templates/{templateId:guid}")]
    public async Task<IActionResult> DeleteTemplate(Guid id, Guid templateId, CancellationToken ct)
    {
        await _sender.Send(new DeleteRoleTemplateCommand(id, templateId), ct);
        return NoContent();
    }
}

public sealed record CreateBusinessTypeRequest(string Name, string? Description = null);
public sealed record UpdateBusinessTypeRequest(string Name, string? Description);
public sealed record RoleTemplateRequest(string Name, string? Description, IReadOnlyList<string> Permissions);
