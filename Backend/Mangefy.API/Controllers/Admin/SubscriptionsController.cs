using Mangefy.API.Filters;
using Mangefy.Application.Platform.Subscriptions.Commands.ConfirmPayment;
using Mangefy.Application.Platform.Subscriptions.Commands.CreateSubscription;
using Mangefy.Application.Platform.Subscriptions.Commands.GenerateInvoice;
using Mangefy.Application.Platform.Subscriptions.Queries.GetOverdueSubscriptions;
using Mangefy.Application.Platform.Subscriptions.Queries.GetSubscriptionByTenant;
using Mangefy.Application.Platform.Subscriptions.Queries.ListSubscriptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers.Admin;

[ApiController]
[Route("api/admin/subscriptions")]
[Authorize]
[RequireAdminSaas]
public sealed class SubscriptionsController : ControllerBase
{
    private readonly ISender _sender;
    public SubscriptionsController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSubscriptionRequest request, CancellationToken ct)
    {
        var id = await _sender.Send(new CreateSubscriptionCommand(request.TenantId, request.PlanId, request.StartDate, request.NextDueDate), ct);
        return Ok(new { id });
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed([FromBody] CreateSubscriptionRequest request, CancellationToken ct)
    {
        var id = await _sender.Send(new CreateSubscriptionCommand(request.TenantId, request.PlanId, request.StartDate, request.NextDueDate), ct);
        return Ok(new { id });
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
        => Ok(await _sender.Send(new ListSubscriptionsQuery(), ct));

    [HttpGet("by-tenant/{tenantId:guid}")]
    public async Task<IActionResult> ByTenant(Guid tenantId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetSubscriptionByTenantQuery(tenantId), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("overdue")]
    public async Task<IActionResult> Overdue(CancellationToken ct)
        => Ok(await _sender.Send(new GetOverdueSubscriptionsQuery(), ct));

    [HttpPost("{id:guid}/invoices")]
    public async Task<IActionResult> GenerateInvoice(Guid id, [FromBody] GenerateInvoiceRequest request, CancellationToken ct)
    {
        var invoiceId = await _sender.Send(new GenerateInvoiceCommand(id, request.Amount, request.DueDate), ct);
        return Ok(new { invoiceId });
    }

    [HttpPatch("{id:guid}/invoices/{invoiceId:guid}/confirm")]
    public async Task<IActionResult> ConfirmPayment(Guid id, Guid invoiceId, [FromBody] ConfirmPaymentRequest request, CancellationToken ct)
    {
        await _sender.Send(new ConfirmPaymentCommand(id, invoiceId, request.PaidAt, request.NextDueDate,
            request.PaymentReference, request.Notes), ct);
        return NoContent();
    }
}

public sealed record CreateSubscriptionRequest(Guid TenantId, Guid PlanId, DateOnly StartDate, DateOnly NextDueDate);
public sealed record GenerateInvoiceRequest(decimal Amount, DateOnly DueDate);
public sealed record ConfirmPaymentRequest(DateOnly PaidAt, DateOnly NextDueDate, string? PaymentReference, string? Notes);
