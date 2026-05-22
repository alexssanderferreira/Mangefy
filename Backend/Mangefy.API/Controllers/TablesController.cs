using Mangefy.API.Filters;
using Mangefy.Application.Tables.Commands.CreateTable;
using Mangefy.Application.Tables.Commands.SetTableStatus;
using Mangefy.Application.Tables.Commands.UpdateTable;
using Mangefy.Application.Tables.Queries.GetTablesByTenant;
using Mangefy.Domain.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/[controller]")]
[Authorize]
[ValidateTenantAccess]
public sealed class TablesController : ControllerBase
{
    private readonly ISender _sender;
    public TablesController(ISender sender) => _sender = sender;

    [HttpGet]
    [RequirePermission(PermissionCatalog.Tables.Read)]
    public async Task<IActionResult> GetAll(Guid tenantId, CancellationToken ct)
        => Ok(await _sender.Send(new GetTablesByTenantQuery(tenantId), ct));

    [HttpPost]
    [RequirePermission(PermissionCatalog.Tables.Manage)]
    public async Task<IActionResult> Create(Guid tenantId, [FromBody] CreateTableRequest request, CancellationToken ct)
    {
        var id = await _sender.Send(new CreateTableCommand(tenantId, request.Number, request.Capacity, request.Section), ct);
        return Created($"/api/tenants/{tenantId}/tables/{id}", new { id });
    }

    [HttpPut("{tableId:guid}")]
    [RequirePermission(PermissionCatalog.Tables.Manage)]
    public async Task<IActionResult> Update(Guid tenantId, Guid tableId, [FromBody] UpdateTableRequest request, CancellationToken ct)
    {
        await _sender.Send(new UpdateTableCommand(tenantId, tableId, request.Number, request.Capacity, request.Section), ct);
        return NoContent();
    }

    [HttpPatch("{tableId:guid}/status")]
    [RequirePermission(PermissionCatalog.Tables.Manage)]
    public async Task<IActionResult> SetStatus(Guid tenantId, Guid tableId, [FromBody] SetTableStatusRequest request, CancellationToken ct)
    {
        await _sender.Send(new SetTableStatusCommand(tenantId, tableId, request.Status), ct);
        return NoContent();
    }
}

public sealed record CreateTableRequest(string Number, int Capacity, string? Section);
public sealed record UpdateTableRequest(string Number, int Capacity, string? Section);
public sealed record SetTableStatusRequest(string Status);
