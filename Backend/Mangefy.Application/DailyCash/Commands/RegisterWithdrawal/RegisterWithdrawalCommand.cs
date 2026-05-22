using MediatR;

namespace Mangefy.Application.DailyCash.Commands.RegisterWithdrawal;

public sealed record RegisterWithdrawalCommand(
    Guid TenantId,
    decimal Amount,
    string Reason) : IRequest;
