using Mangefy.Domain.DailyCash;
using Mangefy.Domain.Tabs;
using MediatR;

namespace Mangefy.Application.DailyCash.Commands.CloseCashRegister;

public sealed record MethodBalanceDto(PaymentMethod Method, decimal ExpectedAmount, decimal CountedAmount);

public sealed record CloseCashRegisterCommand(
    Guid TenantId,
    IReadOnlyList<MethodBalanceDto> MethodBalances,
    Guid EmployeeId,
    string? Notes
) : IRequest;
