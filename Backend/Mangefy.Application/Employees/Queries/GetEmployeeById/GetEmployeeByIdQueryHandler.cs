using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Employees.Queries.GetEmployeesByTenant;
using Mangefy.Domain.Employees.Repositories;
using MediatR;

namespace Mangefy.Application.Employees.Queries.GetEmployeeById;

public sealed class GetEmployeeByIdQueryHandler
    : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    private readonly IEmployeeRepository _employees;

    public GetEmployeeByIdQueryHandler(IEmployeeRepository employees)
        => _employees = employees;

    public async Task<EmployeeDto> Handle(
        GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var employee = await _employees.GetByIdAsync(request.EmployeeId, cancellationToken)
            ?? throw new NotFoundException("Funcionário", request.EmployeeId);

        if (employee.TenantId != request.TenantId)
            throw new ForbiddenException();

        return EmployeeDto.FromDomain(employee);
    }
}
