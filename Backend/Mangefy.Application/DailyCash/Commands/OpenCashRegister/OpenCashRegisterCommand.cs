using MediatR;

namespace Mangefy.Application.DailyCash.Commands.OpenCashRegister;

public sealed record OpenCashRegisterCommand(
    Guid TenantId,
    decimal OpeningAmount,
    Guid EmployeeId
) : IRequest<Guid>;
