using Mangefy.Application.Employees.Queries.GetEmployeesByTenant;
using MediatR;

namespace Mangefy.Application.Employees.Queries.GetEmployeeById;

public sealed record GetEmployeeByIdQuery(Guid TenantId, Guid EmployeeId)
    : IRequest<EmployeeDto>;
