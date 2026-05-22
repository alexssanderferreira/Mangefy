using Mangefy.Application.Common.Behaviors;
using Mangefy.Domain.Tabs;
using MediatR;

namespace Mangefy.Application.Tabs.Commands.CloseTab;

public sealed record PaymentRequest(
    decimal Amount,
    PaymentMethod Method,
    decimal ChangeGiven = 0m,
    string? ExternalReference = null);

public sealed record CloseTabCommand(
    Guid TenantId,
    Guid TabId,
    IReadOnlyList<PaymentRequest> Payments,
    decimal DiscountAmount = 0m,
    string? DiscountReason = null,
    decimal ServiceFee = 0m,
    decimal Tip = 0m,
    Guid? ClientCommandId = null
) : IRequest, IIdempotentCommand;
