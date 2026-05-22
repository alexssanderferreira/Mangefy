using MediatR;

namespace Mangefy.Application.Employees.Queries.GetActiveEmployees;

public sealed record GetActiveEmployeesQuery(Guid TenantId) : IRequest<IReadOnlyList<ActiveEmployeeDto>>;
