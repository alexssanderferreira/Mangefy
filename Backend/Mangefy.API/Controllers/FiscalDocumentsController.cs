using Mangefy.API.Filters;
using Mangefy.Application.Fiscal.Commands.CancelFiscalDocument;
using Mangefy.Application.Fiscal.Queries.GetFiscalDocuments;
using Mangefy.Domain.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/fiscal-documents")]
[Authorize]
[ValidateTenantAccess]
public sealed class FiscalDocumentsController : ControllerBase
{
    private readonly ISender _sender;
    public FiscalDocumentsController(ISender sender) => _sender = sender;

    [HttpGet]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> Get(
        Guid tenantId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
        => Ok(await _sender.Send(new GetFiscalDocumentsQuery(tenantId, from, to), ct));

    [HttpPatch("{id:guid}/cancel")]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> Cancel(
        Guid tenantId,
        Guid id,
        [FromBody] CancelFiscalDocumentRequest request,
        CancellationToken ct)
    {
        await _sender.Send(new CancelFiscalDocumentCommand(tenantId, id, request.Reason), ct);
        return NoContent();
    }
}

public sealed record CancelFiscalDocumentRequest(string Reason);
