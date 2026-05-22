using Mangefy.API.Filters;
using Mangefy.Application.Platform.Plans.Commands.ActivatePlan;
using Mangefy.Application.Platform.Plans.Commands.CreatePlan;
using Mangefy.Application.Platform.Plans.Commands.DeactivatePlan;
using Mangefy.Application.Platform.Plans.Commands.DeletePlan;
using Mangefy.Application.Platform.Plans.Commands.UpdatePlan;
using Mangefy.Application.Platform.Plans.Queries.GetPlans;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers.Admin;

[ApiController]
[Route("api/admin/plans")]
[Authorize]
[RequireAdminSaas]
public sealed class PlansController : ControllerBase
{
    private readonly ISender _sender;

    public PlansController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _sender.Send(new GetPlansQuery(), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlanRequest request, CancellationToken ct)
    {
        var id = await _sender.Send(new CreatePlanCommand(
            request.Name, request.MonthlyPrice, request.MaxTables,
            request.MaxMenuItems, request.MaxUsers, request.MaxCustomRoles,
            request.Description), ct);
        return Created($"/api/admin/plans/{id}", new { Id = id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePlanRequest request, CancellationToken ct)
    {
        await _sender.Send(new UpdatePlanCommand(
            id, request.MonthlyPrice, request.MaxTables,
            request.MaxMenuItems, request.MaxUsers, request.MaxCustomRoles,
            request.Description), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await _sender.Send(new ActivatePlanCommand(id), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeactivatePlanCommand(id), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeletePlanCommand(id), ct);
        return NoContent();
    }
}

public sealed record CreatePlanRequest(
    string Name,
    decimal MonthlyPrice,
    int MaxTables,
    int MaxMenuItems,
    int MaxUsers,
    int MaxCustomRoles = 0,
    string? Description = null);

public sealed record UpdatePlanRequest(
    decimal MonthlyPrice,
    int MaxTables,
    int MaxMenuItems,
    int MaxUsers,
    int MaxCustomRoles,
    string? Description = null);
