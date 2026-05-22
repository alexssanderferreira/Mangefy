using Mangefy.API.Filters;
using Mangefy.Application.Roles.Commands.CreateRole;
using Mangefy.Application.Roles.Commands.UpdateRolePermissions;
using Mangefy.Application.Roles.Queries.GetRolesByTenant;
using Mangefy.Domain.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/[controller]")]
[Authorize]
[ValidateTenantAccess]
public sealed class RolesController : ControllerBase
{
    private readonly ISender _sender;
    public RolesController(ISender sender) => _sender = sender;

    [HttpGet]
    [RequirePermission(PermissionCatalog.Roles.Read)]
    public async Task<IActionResult> GetAll(Guid tenantId, CancellationToken ct)
        => Ok(await _sender.Send(new GetRolesByTenantQuery(tenantId), ct));

    [HttpPost]
    [RequirePermission(PermissionCatalog.Roles.Manage)]
    public async Task<IActionResult> Create(Guid tenantId, [FromBody] CreateRoleRequest request, CancellationToken ct)
    {
        var id = await _sender.Send(new CreateRoleCommand(tenantId, request.Name, request.Description, request.Permissions), ct);
        return Created($"/api/tenants/{tenantId}/roles/{id}", new { id });
    }

    [HttpPut("{roleId:guid}/permissions")]
    [RequirePermission(PermissionCatalog.Roles.Manage)]
    public async Task<IActionResult> UpdatePermissions(Guid tenantId, Guid roleId, [FromBody] UpdateRolePermissionsRequest request, CancellationToken ct)
    {
        await _sender.Send(new UpdateRolePermissionsCommand(tenantId, roleId, request.Permissions), ct);
        return NoContent();
    }
}

public sealed record CreateRoleRequest(string Name, string? Description, IReadOnlyList<string> Permissions);
public sealed record UpdateRolePermissionsRequest(IReadOnlyList<string> Permissions);
