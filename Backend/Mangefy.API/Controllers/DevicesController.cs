using Mangefy.API.Filters;
using Mangefy.Application.Devices.Commands.DeactivateDevice;
using Mangefy.Application.Devices.Commands.RegisterDevice;
using Mangefy.Application.Devices.Queries.GetDevices;
using Mangefy.Domain.Devices;
using Mangefy.Domain.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/devices")]
[Authorize]
[ValidateTenantAccess]
public sealed class DevicesController : ControllerBase
{
    private readonly ISender _sender;

    public DevicesController(ISender sender) => _sender = sender;

    [HttpGet]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> GetAll(Guid tenantId, CancellationToken ct)
        => Ok(await _sender.Send(new GetDevicesQuery(tenantId), ct));

    [HttpPost]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> Register(
        Guid tenantId,
        [FromBody] RegisterDeviceRequest request,
        CancellationToken ct)
    {
        var id = await _sender.Send(new RegisterDeviceCommand(tenantId, request.Name, request.Type, request.PublicIdentifier), ct);
        return Created($"/api/tenants/{tenantId}/devices/{id}", new { Id = id });
    }

    [HttpPatch("{id:guid}/deactivate")]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> Deactivate(Guid tenantId, Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeactivateDeviceCommand(tenantId, id), ct);
        return NoContent();
    }
}

public sealed record RegisterDeviceRequest(string Name, DeviceType Type, string PublicIdentifier);
