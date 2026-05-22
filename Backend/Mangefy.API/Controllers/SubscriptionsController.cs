using Mangefy.API.Filters;
using Mangefy.Application.Subscriptions.Commands.ConfirmPayment;
using Mangefy.Application.Subscriptions.Commands.CreateSubscription;
using Mangefy.Application.Subscriptions.Commands.GenerateInvoice;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/subscription")]
[Authorize]
[RequireAdminSaas]
public sealed class SubscriptionsController : ControllerBase
{
    private readonly ISender _sender;
    public SubscriptionsController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create(Guid tenantId, [FromBody] CreateSubscriptionRequest request, CancellationToken ct)
    {
        var id = await _sender.Send(new CreateSubscriptionCommand(tenantId, request.PlanId, request.StartDate, request.NextDueDate), ct);
        return Created($"/api/tenants/{tenantId}/subscription/{id}", new { id });
    }

    [HttpPost("invoices")]
    public async Task<IActionResult> GenerateInvoice(Guid tenantId, [FromBody] GenerateInvoiceRequest request, CancellationToken ct)
    {
        var id = await _sender.Send(new GenerateInvoiceCommand(tenantId, request.Amount, request.DueDate), ct);
        return Created(string.Empty, new { id });
    }

    [HttpPost("invoices/{invoiceId:guid}/confirm")]
    public async Task<IActionResult> ConfirmPayment(Guid tenantId, Guid invoiceId, [FromBody] ConfirmPaymentRequest request, CancellationToken ct)
    {
        await _sender.Send(new ConfirmPaymentCommand(
            tenantId, invoiceId, request.PaidAt, request.NextDueDate, request.PaymentReference, request.Notes), ct);
        return NoContent();
    }
}

public sealed record CreateSubscriptionRequest(Guid PlanId, DateOnly StartDate, DateOnly NextDueDate);
public sealed record GenerateInvoiceRequest(decimal Amount, DateOnly DueDate);
public sealed record ConfirmPaymentRequest(DateOnly PaidAt, DateOnly NextDueDate, string? PaymentReference, string? Notes);
