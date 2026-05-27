using Mangefy.API.Filters;
using Mangefy.Application.Employees.Queries.GetEmployeesByTenant;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers.Admin;

[ApiController]
[Route("api/admin/tenants")]
[Authorize]
[RequireAdminSaas]
public sealed class AdminTenantsController : ControllerBase
{
    private readonly ISender _sender;
    public AdminTenantsController(ISender sender) => _sender = sender;

    [HttpGet("{id:guid}/employees")]
    public async Task<IActionResult> GetEmployees(Guid id, CancellationToken ct)
        => Ok(await _sender.Send(new GetEmployeesByTenantQuery(id), ct));
}
