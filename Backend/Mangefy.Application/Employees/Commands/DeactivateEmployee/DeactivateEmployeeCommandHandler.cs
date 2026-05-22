using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Employees;
using Mangefy.Domain.Employees.Repositories;
using MediatR;

namespace Mangefy.Application.Employees.Commands.DeactivateEmployee;

public sealed class DeactivateEmployeeCommandHandler : IRequestHandler<DeactivateEmployeeCommand>
{
    private readonly IEmployeeRepository _employees;
    private readonly IUnitOfWork _uow;

    public DeactivateEmployeeCommandHandler(IEmployeeRepository employees, IUnitOfWork uow)
    {
        _employees = employees;
        _uow = uow;
    }

    public async Task Handle(DeactivateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employees.GetByIdAsync(request.EmployeeId, cancellationToken)
            ?? throw new NotFoundException(nameof(Employee), request.EmployeeId);

        if (employee.TenantId != request.TenantId)
            throw new ForbiddenException();

        employee.Deactivate();
        await _employees.UpdateAsync(employee, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
