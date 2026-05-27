using MediatR;

namespace Mangefy.Application.Platform.Subscriptions.Commands.ConfirmPayment;

public sealed record ConfirmPaymentCommand(
    Guid SubscriptionId,
    Guid InvoiceId,
    DateOnly PaidAt,
    DateOnly NextDueDate,
    string? PaymentReference,
    string? Notes) : IRequest;
