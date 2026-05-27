using Mangefy.API.Filters;
using Mangefy.Application.Tenants.Commands.CancelTenant;
using Mangefy.Application.Tenants.Commands.ChangeBusinessType;
using Mangefy.Application.Tenants.Commands.ChangeTenantPlan;
using Mangefy.Application.Tenants.Commands.CreateTenant;
using Mangefy.Application.Tenants.Commands.ReactivateTenant;
using Mangefy.Application.Tenants.Commands.SuspendTenant;
using Mangefy.Application.Tenants.Commands.UpdateTenant;
using Mangefy.Application.Employees.Queries.GetEmployeesByTenant;
using Mangefy.Application.Tenants.Queries.GetTenantById;
using Mangefy.Application.Tenants.Queries.ListTenants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[RequireAdminSaas]
public sealed class TenantsController : ControllerBase
{
    private readonly ISender _sender;

    public TenantsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        => Ok(await _sender.Send(new ListTenantsQuery(page, pageSize), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await _sender.Send(new GetTenantByIdQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request, CancellationToken ct)
    {
        var id = await _sender.Send(new CreateTenantCommand(
            request.OwnerId, request.Name, request.Slug,
            request.PlanId, request.BusinessTypeId,
            request.Timezone, request.TrialDays, request.Email), ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantRequest request, CancellationToken ct)
    {
        await _sender.Send(new UpdateTenantCommand(id, request.Name, request.LogoUrl, request.Email, request.Timezone,
            request.Phone, request.Cep, request.Logradouro, request.Numero,
            request.Complemento, request.Bairro, request.Cidade, request.Uf), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/plan")]
    public async Task<IActionResult> ChangePlan(Guid id, [FromBody] ChangePlanRequest request, CancellationToken ct)
    {
        await _sender.Send(new ChangeTenantPlanCommand(id, request.NewPlanId), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/business-type")]
    public async Task<IActionResult> ChangeBusinessType(Guid id, [FromBody] ChangeBusinessTypeRequest request, CancellationToken ct)
    {
        await _sender.Send(new ChangeBusinessTypeCommand(id, request.BusinessTypeId), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken ct)
    {
        await _sender.Send(new SuspendTenantCommand(id), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
    {
        await _sender.Send(new ReactivateTenantCommand(id), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await _sender.Send(new CancelTenantCommand(id), ct);
        return NoContent();
    }

}

public sealed record UpdateTenantRequest(
    string Name, string? LogoUrl, string? Email, string? Timezone,
    string? Phone, string? Cep, string? Logradouro, string? Numero,
    string? Complemento, string? Bairro, string? Cidade, string? Uf);
public sealed record ChangePlanRequest(Guid NewPlanId);
public sealed record ChangeBusinessTypeRequest(Guid BusinessTypeId);

public sealed record CreateTenantRequest(
    Guid OwnerId,
    string Name,
    string Slug,
    Guid PlanId,
    Guid BusinessTypeId,
    string Timezone = "America/Sao_Paulo",
    int TrialDays = 14,
    string? Email = null);
