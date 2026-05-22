using Mangefy.API.Filters;
using Mangefy.Application.DailyCash.Commands.CloseCashRegister;
using Mangefy.Application.DailyCash.Commands.OpenCashRegister;
using Mangefy.Domain.Tabs;
using Mangefy.Application.DailyCash.Commands.RegisterWithdrawal;
using Mangefy.Application.DailyCash.Queries.GetCashRegisterHistory;
using Mangefy.Application.DailyCash.Queries.GetCurrentCashRegister;
using Mangefy.Domain.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/cash-registers")]
[Authorize]
[ValidateTenantAccess]
public sealed class CashRegistersController : ControllerBase
{
    private readonly ISender _sender;
    public CashRegistersController(ISender sender) => _sender = sender;

    [HttpGet("current")]
    [RequirePermission(PermissionCatalog.Cash.Manage)]
    public async Task<IActionResult> GetCurrent(Guid tenantId, CancellationToken ct)
        => Ok(await _sender.Send(new GetCurrentCashRegisterQuery(tenantId), ct));

    [HttpGet("history")]
    [RequirePermission(PermissionCatalog.Cash.Manage)]
    public async Task<IActionResult> GetHistory(Guid tenantId, [FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
        => Ok(await _sender.Send(new GetCashRegisterHistoryQuery(tenantId, from, to), ct));

    [HttpPost("open")]
    [RequirePermission(PermissionCatalog.Cash.Manage)]
    public async Task<IActionResult> Open(Guid tenantId, [FromBody] OpenCashRegisterRequest request, CancellationToken ct)
    {
        var id = await _sender.Send(new OpenCashRegisterCommand(tenantId, request.OpeningAmount, request.EmployeeId), ct);
        return Created($"/api/tenants/{tenantId}/cash-registers/{id}", new { id });
    }

    [HttpPost("close")]
    [RequirePermission(PermissionCatalog.Cash.Manage)]
    public async Task<IActionResult> Close(Guid tenantId, [FromBody] CloseCashRegisterRequest request, CancellationToken ct)
    {
        var balances = request.MethodBalances
            .Select(m => new MethodBalanceDto(m.Method, m.ExpectedAmount, m.CountedAmount))
            .ToList();
        await _sender.Send(new CloseCashRegisterCommand(tenantId, balances, request.EmployeeId, request.Notes), ct);
        return NoContent();
    }

    [HttpPost("withdrawal")]
    [RequirePermission(PermissionCatalog.Cash.Manage)]
    public async Task<IActionResult> RegisterWithdrawal(Guid tenantId, [FromBody] RegisterWithdrawalRequest request, CancellationToken ct)
    {
        await _sender.Send(new RegisterWithdrawalCommand(tenantId, request.Amount, request.Reason), ct);
        return NoContent();
    }
}

public sealed record OpenCashRegisterRequest(decimal OpeningAmount, Guid EmployeeId);
public sealed record CloseCashRegisterMethodBalance(PaymentMethod Method, decimal ExpectedAmount, decimal CountedAmount);
public sealed record CloseCashRegisterRequest(IReadOnlyList<CloseCashRegisterMethodBalance> MethodBalances, Guid EmployeeId, string? Notes);
public sealed record RegisterWithdrawalRequest(decimal Amount, string Reason);
