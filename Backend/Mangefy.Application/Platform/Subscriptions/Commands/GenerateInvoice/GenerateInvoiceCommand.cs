using MediatR;

namespace Mangefy.Application.Platform.Subscriptions.Commands.GenerateInvoice;

public sealed record GenerateInvoiceCommand(
    Guid SubscriptionId,
    decimal Amount,
    DateOnly DueDate) : IRequest<Guid>;
