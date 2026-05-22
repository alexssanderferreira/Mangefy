using Mangefy.API.Filters;
using Mangefy.Application.Settings.BusinessSchedule.Commands.UpdateBusinessSchedule;
using Mangefy.Application.Settings.BusinessSchedule.Queries.GetBusinessSchedule;
using Mangefy.Application.Settings.EmployeeSchedule.Commands.UpdateEmployeeSchedule;
using Mangefy.Application.Settings.EmployeeSchedule.Queries.GetEmployeeSchedule;
using Mangefy.Application.Settings.FiscalSettings.Commands.UpdateFiscalSettings;
using Mangefy.Application.Settings.FiscalSettings.Queries.GetFiscalSettings;
using Mangefy.Application.Settings.PaymentSettings.Commands.UpdatePaymentSettings;
using Mangefy.Application.Settings.PaymentSettings.Queries.GetPaymentSettings;
using Mangefy.Application.Settings.PrinterSettings.Commands.AddPrinter;
using Mangefy.Application.Settings.PrinterSettings.Commands.RemovePrinter;
using Mangefy.Application.Settings.PrinterSettings.Commands.UpdatePrinter;
using Mangefy.Application.Settings.PrinterSettings.Queries.GetPrinterSettings;
using Mangefy.Application.Settings.ReservationSettings.Commands.UpdateReservationSettings;
using Mangefy.Application.Settings.ReservationSettings.Queries.GetReservationSettings;
using Mangefy.Application.Settings.TabSettings.Commands.UpdateTabSettings;
using Mangefy.Application.Settings.TabSettings.Queries.GetTabSettings;
using Mangefy.Domain.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/settings")]
[Authorize]
[ValidateTenantAccess]
public sealed class SettingsController : ControllerBase
{
    private readonly ISender _sender;
    public SettingsController(ISender sender) => _sender = sender;

    // Payment
    [HttpGet("payment")]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> GetPayment(Guid tenantId, CancellationToken ct)
        => Ok(await _sender.Send(new GetPaymentSettingsQuery(tenantId), ct));

    [HttpPut("payment")]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> UpdatePayment(Guid tenantId, [FromBody] UpdatePaymentRequest request, CancellationToken ct)
    {
        await _sender.Send(new UpdatePaymentSettingsCommand(tenantId, request.EnabledMethods), ct);
        return NoContent();
    }

    // Fiscal
    [HttpGet("fiscal")]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> GetFiscal(Guid tenantId, CancellationToken ct)
        => Ok(await _sender.Send(new GetFiscalSettingsQuery(tenantId), ct));

    [HttpPut("fiscal")]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> UpdateFiscal(Guid tenantId, [FromBody] UpdateFiscalRequest request, CancellationToken ct)
    {
        await _sender.Send(new UpdateFiscalSettingsCommand(tenantId, request.NfceEnabled, request.Cnpj, request.FiscalHubApiKey, request.AutoEmitOnTabClose), ct);
        return NoContent();
    }

    // Printers
    [HttpGet("printers")]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> GetPrinters(Guid tenantId, CancellationToken ct)
        => Ok(await _sender.Send(new GetPrinterSettingsQuery(tenantId), ct));

    [HttpPost("printers")]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> AddPrinter(Guid tenantId, [FromBody] AddPrinterRequest request, CancellationToken ct)
    {
        await _sender.Send(new AddPrinterCommand(tenantId, request.Name, request.IpAddressOrPort, request.Station), ct);
        return NoContent();
    }

    [HttpPut("printers/{printerId:guid}")]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> UpdatePrinter(Guid tenantId, Guid printerId, [FromBody] UpdatePrinterRequest request, CancellationToken ct)
    {
        await _sender.Send(new UpdatePrinterCommand(tenantId, printerId, request.Name, request.IpAddressOrPort, request.Station), ct);
        return NoContent();
    }

    [HttpDelete("printers/{printerId:guid}")]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> RemovePrinter(Guid tenantId, Guid printerId, CancellationToken ct)
    {
        await _sender.Send(new RemovePrinterCommand(tenantId, printerId), ct);
        return NoContent();
    }

    // Tab
    [HttpGet("tab")]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> GetTab(Guid tenantId, CancellationToken ct)
        => Ok(await _sender.Send(new GetTabSettingsQuery(tenantId), ct));

    [HttpPut("tab")]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> UpdateTab(Guid tenantId, [FromBody] UpdateTabSettingsRequest request, CancellationToken ct)
    {
        await _sender.Send(new UpdateTabSettingsCommand(tenantId, request.MinTabNumber, request.MaxTabNumber, request.MaxDiscountPercent, request.DiscountReasonRequiredAbove), ct);
        return NoContent();
    }

    // Business Schedule
    [HttpGet("schedule")]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> GetSchedule(Guid tenantId, CancellationToken ct)
        => Ok(await _sender.Send(new GetBusinessScheduleQuery(tenantId), ct));

    [HttpPut("schedule")]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> UpdateSchedule(Guid tenantId, [FromBody] UpdateBusinessScheduleRequest request, CancellationToken ct)
    {
        await _sender.Send(new UpdateBusinessScheduleCommand(tenantId, request.WeeklySchedule, request.SpecialDays, request.ClosingPolicy), ct);
        return NoContent();
    }

    // Reservation settings
    [HttpGet("reservations")]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> GetReservationSettings(Guid tenantId, CancellationToken ct)
        => Ok(await _sender.Send(new GetReservationSettingsQuery(tenantId), ct));

    [HttpPut("reservations")]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> UpdateReservationSettings(Guid tenantId, [FromBody] UpdateReservationSettingsRequest request, CancellationToken ct)
    {
        await _sender.Send(new UpdateReservationSettingsCommand(tenantId, request.MaxSimultaneousReservations), ct);
        return NoContent();
    }

    // Employee Schedule
    [HttpGet("employees/{employeeId:guid}/schedule")]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> GetEmployeeSchedule(Guid tenantId, Guid employeeId, CancellationToken ct)
        => Ok(await _sender.Send(new GetEmployeeScheduleQuery(tenantId, employeeId), ct));

    [HttpPut("employees/{employeeId:guid}/schedule")]
    [RequirePermission(PermissionCatalog.Settings.Manage)]
    public async Task<IActionResult> UpdateEmployeeSchedule(Guid tenantId, Guid employeeId, [FromBody] UpdateEmployeeScheduleRequest request, CancellationToken ct)
    {
        await _sender.Send(new UpdateEmployeeScheduleCommand(tenantId, employeeId, request.WeeklyShifts), ct);
        return NoContent();
    }
}

public sealed record UpdatePaymentRequest(IReadOnlyList<string> EnabledMethods);
public sealed record UpdateFiscalRequest(bool NfceEnabled, string? Cnpj, string? FiscalHubApiKey, bool AutoEmitOnTabClose);
public sealed record AddPrinterRequest(string Name, string IpAddressOrPort, string Station);
public sealed record UpdatePrinterRequest(string Name, string IpAddressOrPort, string Station);
public sealed record UpdateTabSettingsRequest(
    int MinTabNumber,
    int MaxTabNumber,
    decimal MaxDiscountPercent = 10m,
    decimal? DiscountReasonRequiredAbove = null);
public sealed record UpdateReservationSettingsRequest(int? MaxSimultaneousReservations);
public sealed record UpdateBusinessScheduleRequest(
    IReadOnlyList<DayScheduleInput>? WeeklySchedule,
    IReadOnlyList<SpecialDayInput>? SpecialDays,
    ClosingPolicyInput? ClosingPolicy);
public sealed record UpdateEmployeeScheduleRequest(IReadOnlyList<DayShiftInput> WeeklyShifts);
