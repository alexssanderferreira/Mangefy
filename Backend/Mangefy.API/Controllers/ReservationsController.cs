using Mangefy.API.Filters;
using Mangefy.Application.Reservations.Commands.CancelReservation;
using Mangefy.Application.Reservations.Commands.ConfirmReservation;
using Mangefy.Application.Reservations.Commands.CreateReservation;
using Mangefy.Application.Reservations.Commands.MarkNoShow;
using Mangefy.Application.Reservations.Commands.RegisterArrival;
using Mangefy.Application.Reservations.Queries.GetReservationsByDate;
using Mangefy.Domain.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/[controller]")]
[Authorize]
[ValidateTenantAccess]
public sealed class ReservationsController : ControllerBase
{
    private readonly ISender _sender;
    public ReservationsController(ISender sender) => _sender = sender;

    [HttpGet]
    [RequirePermission(PermissionCatalog.Reservations.Read)]
    public async Task<IActionResult> GetByDate(Guid tenantId, [FromQuery] DateOnly date, CancellationToken ct)
        => Ok(await _sender.Send(new GetReservationsByDateQuery(tenantId, date), ct));

    [HttpPost]
    [RequirePermission(PermissionCatalog.Reservations.Manage)]
    public async Task<IActionResult> Create(Guid tenantId, [FromBody] CreateReservationRequest request, CancellationToken ct)
    {
        var id = await _sender.Send(new CreateReservationCommand(
            tenantId, request.EmployeeId, request.CustomerName, request.CustomerPhone,
            request.PartySize, request.Date, request.Time, request.TableId, request.Notes), ct);
        return Created($"/api/tenants/{tenantId}/reservations/{id}", new { id });
    }

    [HttpPatch("{reservationId:guid}/confirm")]
    [RequirePermission(PermissionCatalog.Reservations.Manage)]
    public async Task<IActionResult> Confirm(Guid tenantId, Guid reservationId, CancellationToken ct)
    {
        await _sender.Send(new ConfirmReservationCommand(tenantId, reservationId), ct);
        return NoContent();
    }

    [HttpPatch("{reservationId:guid}/cancel")]
    [RequirePermission(PermissionCatalog.Reservations.Manage)]
    public async Task<IActionResult> Cancel(Guid tenantId, Guid reservationId, [FromBody] CancelReservationRequest request, CancellationToken ct)
    {
        await _sender.Send(new CancelReservationCommand(tenantId, reservationId, request.Reason), ct);
        return NoContent();
    }

    [HttpPatch("{reservationId:guid}/no-show")]
    [RequirePermission(PermissionCatalog.Reservations.Manage)]
    public async Task<IActionResult> MarkNoShow(Guid tenantId, Guid reservationId, CancellationToken ct)
    {
        await _sender.Send(new MarkNoShowCommand(tenantId, reservationId), ct);
        return NoContent();
    }

    [HttpPatch("{reservationId:guid}/arrival")]
    [RequirePermission(PermissionCatalog.Reservations.Manage)]
    public async Task<IActionResult> RegisterArrival(Guid tenantId, Guid reservationId, [FromBody] ArrivalRequest request, CancellationToken ct)
    {
        var tabId = await _sender.Send(new RegisterArrivalCommand(tenantId, reservationId, request.EmployeeId), ct);
        return Ok(new { tabId });
    }
}

public sealed record CreateReservationRequest(
    Guid EmployeeId, string CustomerName, string? CustomerPhone,
    int PartySize, DateOnly Date, TimeOnly Time, Guid? TableId, string? Notes);
public sealed record CancelReservationRequest(string Reason);
public sealed record ArrivalRequest(Guid EmployeeId);
