using MediatR;

namespace Mangefy.Application.Subscriptions.Commands.ConfirmPayment;

public sealed record ConfirmPaymentCommand(
    Guid TenantId,
    Guid InvoiceId,
    DateOnly PaidAt,
    DateOnly NextDueDate,
    string? PaymentReference,
    string? Notes
) : IRequest;
