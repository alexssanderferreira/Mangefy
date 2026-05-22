using Mangefy.API.Filters;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Application.Reports.Queries.GetOperationalReport;
using Mangefy.Application.Reports.Queries.GetSalesReport;
using Mangefy.Domain.Platform.Features;
using Mangefy.Domain.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/[controller]")]
[Authorize]
[ValidateTenantAccess]
public sealed class ReportsController : ControllerBase
{
    private readonly ISender _sender;
    public ReportsController(ISender sender) => _sender = sender;

    [HttpGet("sales")]
    [RequirePermission(PermissionCatalog.Reports.Read)]
    public async Task<IActionResult> GetSales(
        Guid tenantId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
        => Ok(await _sender.Send(new GetSalesReportQuery(tenantId, from, to), ct));

    [HttpGet("operational")]
    [RequirePermission(PermissionCatalog.Reports.Read)]
    public async Task<IActionResult> GetOperational(Guid tenantId, CancellationToken ct)
        => Ok(await _sender.Send(new GetOperationalReportQuery(tenantId), ct));

    /// <summary>
    /// Relatório avançado (RF-054). Requer feature gate features.reports_advanced.
    /// Implementação do payload é preparatória — retorna estrutura vazia até implementação completa.
    /// </summary>
    [HttpGet("advanced")]
    [RequirePermission(PermissionCatalog.Reports.Read)]
    public async Task<IActionResult> GetAdvanced(
        Guid tenantId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromServices] IFeatureGateService featureGate,
        CancellationToken ct)
    {
        await featureGate.RequireAsync(tenantId, FeatureCatalog.Reports.Advanced, ct);
        // Implementação completa de analytics avançado é futuro (RF-054).
        return Ok(new { Message = "Relatório avançado em desenvolvimento.", TenantId = tenantId, From = from, To = to });
    }
}
