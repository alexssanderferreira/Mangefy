using MediatR;

namespace Mangefy.Application.Employees.Commands.DeactivateEmployee;

public sealed record DeactivateEmployeeCommand(Guid TenantId, Guid EmployeeId) : IRequest;
