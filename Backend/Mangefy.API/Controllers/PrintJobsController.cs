using Mangefy.API.Filters;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Application.PrintJobs.Commands.MarkAsPrinted;
using Mangefy.Application.PrintJobs.Commands.ReprintJob;
using Mangefy.Application.PrintJobs.Queries.GetPendingJobs;
using Mangefy.Domain.Menus;
using Mangefy.Domain.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/print-jobs")]
[Authorize]
[ValidateTenantAccess]
public sealed class PrintJobsController : ControllerBase
{
    private readonly ISender _sender;

    public PrintJobsController(ISender sender) => _sender = sender;

    [HttpGet("pending")]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> GetPending(Guid tenantId, CancellationToken ct)
        => Ok(await _sender.Send(new GetPendingJobsQuery(tenantId), ct));

    [HttpPost("reprint")]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> Reprint(
        Guid tenantId,
        [FromBody] ReprintRequest request,
        [FromServices] ICurrentUser currentUser,
        CancellationToken ct)
    {
        var employeeId = currentUser.EmployeeId ?? Guid.Empty;
        var id = await _sender.Send(
            new ReprintJobCommand(tenantId, employeeId, request.Station, request.Payload, request.Reason, request.PrinterId), ct);
        return Created(string.Empty, new { Id = id });
    }

    [HttpPatch("{id:guid}/printed")]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> MarkPrinted(Guid tenantId, Guid id, CancellationToken ct)
    {
        await _sender.Send(new MarkAsPrintedCommand(tenantId, id), ct);
        return NoContent();
    }
}

public sealed record ReprintRequest(
    MenuItemStation Station,
    string Payload,
    string Reason,
    Guid? PrinterId = null);
