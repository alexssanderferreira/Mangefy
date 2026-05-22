using MediatR;

namespace Mangefy.Application.Employees.Commands.UpdateEmployee;

public sealed record UpdateEmployeeCommand(
    Guid TenantId,
    Guid EmployeeId,
    string Name,
    Guid TenantRoleId
) : IRequest;
