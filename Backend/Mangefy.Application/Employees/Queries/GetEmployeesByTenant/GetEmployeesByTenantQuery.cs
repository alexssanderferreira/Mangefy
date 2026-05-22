using MediatR;

namespace Mangefy.Application.Employees.Queries.GetEmployeesByTenant;

public sealed record GetEmployeesByTenantQuery(Guid TenantId)
    : IRequest<IReadOnlyList<EmployeeDto>>;
