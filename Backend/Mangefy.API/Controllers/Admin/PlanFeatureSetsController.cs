using Mangefy.API.Filters;
using Mangefy.Application.Platform.PlanFeatureSets.Commands.UpsertPlanFeatureSet;
using Mangefy.Application.Platform.PlanFeatureSets.Queries.GetPlanFeatureSets;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers.Admin;

[ApiController]
[Route("api/admin/plans/{planId:guid}/feature-sets")]
[Authorize]
[RequireAdminSaas]
public sealed class PlanFeatureSetsController : ControllerBase
{
    private readonly ISender _sender;

    public PlanFeatureSetsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetByPlan(Guid planId, CancellationToken ct)
        => Ok(await _sender.Send(new GetPlanFeatureSetsQuery(planId), ct));

    [HttpPut("{businessTypeId:guid}")]
    public async Task<IActionResult> Upsert(Guid planId, Guid businessTypeId, [FromBody] UpsertFeatureSetRequest request, CancellationToken ct)
    {
        var id = await _sender.Send(new UpsertPlanFeatureSetCommand(planId, businessTypeId, request.EnabledFeatures), ct);
        return Ok(new { Id = id });
    }
}

public sealed record UpsertFeatureSetRequest(IReadOnlyList<string> EnabledFeatures);
