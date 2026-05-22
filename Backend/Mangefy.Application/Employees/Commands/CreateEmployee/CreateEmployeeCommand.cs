using MediatR;

namespace Mangefy.Application.Employees.Commands.CreateEmployee;

public sealed record CreateEmployeeCommand(
    Guid TenantId,
    string Name,
    string Email,
    Guid TenantRoleId
) : IRequest<CreateEmployeeResult>;

public sealed record CreateEmployeeResult(Guid EmployeeId, string ActivationToken);
