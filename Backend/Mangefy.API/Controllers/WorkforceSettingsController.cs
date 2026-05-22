using Mangefy.API.Filters;
using Mangefy.Application.WorkforceSettings.Commands.UpdateWorkforceSettings;
using Mangefy.Application.WorkforceSettings.Queries.GetWorkforceSettings;
using Mangefy.Domain.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/settings/workforce")]
[Authorize]
[ValidateTenantAccess]
public sealed class WorkforceSettingsController : ControllerBase
{
    private readonly ISender _sender;
    public WorkforceSettingsController(ISender sender) => _sender = sender;

    [HttpGet]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> Get(Guid tenantId, CancellationToken ct)
        => Ok(await _sender.Send(new GetWorkforceSettingsQuery(tenantId), ct));

    [HttpPut]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> Update(Guid tenantId, [FromBody] UpdateWorkforceSettingsRequest request, CancellationToken ct)
    {
        await _sender.Send(new UpdateWorkforceSettingsCommand(tenantId, request.ShiftToleranceMinutes), ct);
        return NoContent();
    }
}

public sealed record UpdateWorkforceSettingsRequest(int ShiftToleranceMinutes);
