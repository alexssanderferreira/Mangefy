using Mangefy.API.Filters;
using Mangefy.Application.AuditLogs.Queries.GetAuditLogs;
using Mangefy.Domain.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/[controller]")]
[Authorize]
[ValidateTenantAccess]
public sealed class AuditLogsController : ControllerBase
{
    private readonly ISender _sender;
    public AuditLogsController(ISender sender) => _sender = sender;

    [HttpGet]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> Get(
        Guid tenantId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
        => Ok(await _sender.Send(new GetAuditLogsQuery(tenantId, from, to), ct));
}
