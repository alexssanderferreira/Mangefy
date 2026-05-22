using Mangefy.API.Filters;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Application.OperationalSessions.Commands.EndSession;
using Mangefy.Application.OperationalSessions.Commands.StartSession;
using Mangefy.Application.OperationalSessions.Queries.GetActiveSessions;
using Mangefy.Domain.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/operational-sessions")]
[Authorize]
[ValidateTenantAccess]
public sealed class OperationalSessionsController : ControllerBase
{
    private readonly ISender _sender;

    public OperationalSessionsController(ISender sender) => _sender = sender;

    [HttpGet("active")]
    [RequirePermission(PermissionCatalog.Employees.Read)]
    public async Task<IActionResult> GetActive(Guid tenantId, CancellationToken ct)
        => Ok(await _sender.Send(new GetActiveSessionsQuery(tenantId), ct));

    [HttpPost("start")]
    public async Task<IActionResult> Start(
        Guid tenantId,
        [FromBody] StartSessionRequest request,
        [FromServices] ICurrentUser currentUser,
        CancellationToken ct)
    {
        var employeeId = currentUser.EmployeeId ?? Guid.Empty;
        var result = await _sender.Send(new StartSessionCommand(tenantId, employeeId, request.DeviceId), ct);
        return Created(string.Empty, new { result.SessionId, result.IsWithinShift });
    }

    [HttpPost("{id:guid}/end")]
    public async Task<IActionResult> End(Guid tenantId, Guid id, CancellationToken ct)
    {
        await _sender.Send(new EndSessionCommand(tenantId, id), ct);
        return NoContent();
    }
}

public sealed record StartSessionRequest(Guid? DeviceId = null);
