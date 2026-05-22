using Mangefy.Domain.Employees.Repositories;
using MediatR;

namespace Mangefy.Application.Employees.Queries.GetEmployeesByTenant;

public sealed class GetEmployeesByTenantQueryHandler
    : IRequestHandler<GetEmployeesByTenantQuery, IReadOnlyList<EmployeeDto>>
{
    private readonly IEmployeeRepository _employees;

    public GetEmployeesByTenantQueryHandler(IEmployeeRepository employees)
        => _employees = employees;

    public async Task<IReadOnlyList<EmployeeDto>> Handle(
        GetEmployeesByTenantQuery request, CancellationToken cancellationToken)
    {
        var employees = await _employees.GetByTenantAsync(request.TenantId, cancellationToken);
        return employees.Select(EmployeeDto.FromDomain).ToList();
    }
}
