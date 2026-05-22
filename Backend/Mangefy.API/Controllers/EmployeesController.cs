using Mangefy.API.Filters;
using Mangefy.Application.Employees.Commands.CreateEmployee;
using Mangefy.Application.Employees.Commands.DeactivateEmployee;
using Mangefy.Application.Employees.Commands.GrantTemporaryAccess;
using Mangefy.Application.Employees.Commands.UpdateEmployee;
using Mangefy.Application.Employees.Queries.GetActiveEmployees;
using Mangefy.Application.Employees.Queries.GetEmployeeById;
using Mangefy.Application.Employees.Queries.GetEmployeesByTenant;
using Mangefy.Domain.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/[controller]")]
[Authorize]
[ValidateTenantAccess]
public sealed class EmployeesController : ControllerBase
{
    private readonly ISender _sender;
    public EmployeesController(ISender sender) => _sender = sender;

    [HttpGet]
    [RequirePermission(PermissionCatalog.Employees.Read)]
    public async Task<IActionResult> GetAll(Guid tenantId, CancellationToken ct)
        => Ok(await _sender.Send(new GetEmployeesByTenantQuery(tenantId), ct));

    [HttpGet("{employeeId:guid}")]
    [RequirePermission(PermissionCatalog.Employees.Read)]
    public async Task<IActionResult> GetById(Guid tenantId, Guid employeeId, CancellationToken ct)
        => Ok(await _sender.Send(new GetEmployeeByIdQuery(tenantId, employeeId), ct));

    [HttpGet("active")]
    [RequirePermission(PermissionCatalog.Employees.Read)]
    public async Task<IActionResult> GetActive(Guid tenantId, CancellationToken ct)
        => Ok(await _sender.Send(new GetActiveEmployeesQuery(tenantId), ct));

    [HttpPost]
    [RequirePermission(PermissionCatalog.Employees.Manage)]
    public async Task<IActionResult> Create(Guid tenantId, [FromBody] CreateEmployeeRequest request, CancellationToken ct)
    {
        var result = await _sender.Send(new CreateEmployeeCommand(
            tenantId, request.Name, request.Email, request.TenantRoleId), ct);
        return Created($"/api/tenants/{tenantId}/employees/{result.EmployeeId}",
            new { result.EmployeeId, result.ActivationToken });
    }

    [HttpPut("{employeeId:guid}")]
    [RequirePermission(PermissionCatalog.Employees.Manage)]
    public async Task<IActionResult> Update(Guid tenantId, Guid employeeId, [FromBody] UpdateEmployeeRequest request, CancellationToken ct)
    {
        await _sender.Send(new UpdateEmployeeCommand(tenantId, employeeId, request.Name, request.TenantRoleId), ct);
        return NoContent();
    }

    [HttpPatch("{employeeId:guid}/deactivate")]
    [RequirePermission(PermissionCatalog.Employees.Manage)]
    public async Task<IActionResult> Deactivate(Guid tenantId, Guid employeeId, CancellationToken ct)
    {
        await _sender.Send(new DeactivateEmployeeCommand(tenantId, employeeId), ct);
        return NoContent();
    }

    [HttpPost("{employeeId:guid}/grant-access")]
    [RequirePermission(PermissionCatalog.Employees.Manage)]
    public async Task<IActionResult> GrantTemporaryAccess(Guid tenantId, Guid employeeId, [FromBody] GrantAccessRequest request, CancellationToken ct)
    {
        await _sender.Send(new GrantTemporaryAccessCommand(tenantId, employeeId, request.ExtensionMinutes), ct);
        return NoContent();
    }
}

public sealed record CreateEmployeeRequest(string Name, string Email, Guid TenantRoleId);
public sealed record UpdateEmployeeRequest(string Name, Guid TenantRoleId);
public sealed record GrantAccessRequest(int ExtensionMinutes);
