using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Employees;
using Mangefy.Domain.Employees.Repositories;
using Mangefy.Domain.Roles.Repositories;
using MediatR;

namespace Mangefy.Application.Employees.Commands.UpdateEmployee;

public sealed class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand>
{
    private readonly IEmployeeRepository _employees;
    private readonly ITenantRoleRepository _roles;
    private readonly IUnitOfWork _uow;

    public UpdateEmployeeCommandHandler(IEmployeeRepository employees, ITenantRoleRepository roles, IUnitOfWork uow)
    {
        _employees = employees;
        _roles = roles;
        _uow = uow;
    }

    public async Task Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employees.GetByIdAsync(request.EmployeeId, cancellationToken)
            ?? throw new NotFoundException(nameof(Employee), request.EmployeeId);

        if (employee.TenantId != request.TenantId)
            throw new ForbiddenException();

        var role = await _roles.GetByIdAsync(request.TenantRoleId, cancellationToken)
            ?? throw new NotFoundException("TenantRole", request.TenantRoleId);

        if (role.TenantId != request.TenantId || !role.IsActive || role.IsOwnerRole)
            throw new ForbiddenException("Cargo inválido ou inacessível.");

        employee.UpdateProfile(request.Name);
        employee.AssignRole(request.TenantRoleId);
        await _employees.UpdateAsync(employee, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
