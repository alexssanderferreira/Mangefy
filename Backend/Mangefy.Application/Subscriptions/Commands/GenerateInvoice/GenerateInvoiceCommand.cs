using MediatR;

namespace Mangefy.Application.Subscriptions.Commands.GenerateInvoice;

public sealed record GenerateInvoiceCommand(
    Guid TenantId,
    decimal Amount,
    DateOnly DueDate
) : IRequest<Guid>;
