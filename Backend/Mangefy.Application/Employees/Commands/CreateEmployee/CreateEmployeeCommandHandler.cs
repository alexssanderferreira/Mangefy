using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Employees;
using Mangefy.Domain.Employees.Repositories;
using Mangefy.Domain.Plans;
using Mangefy.Domain.Plans.Repositories;
using Mangefy.Domain.Roles.Repositories;
using Mangefy.Domain.Tenants;
using Mangefy.Domain.Tenants.Repositories;
using MediatR;

namespace Mangefy.Application.Employees.Commands.CreateEmployee;

public sealed class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, CreateEmployeeResult>
{
    private readonly IEmployeeRepository _employees;
    private readonly ITenantRoleRepository _roles;
    private readonly ITenantRepository _tenants;
    private readonly IPlanRepository _plans;
    private readonly IActivationTokenRepository _activationTokens;
    private readonly IUnitOfWork _uow;

    public CreateEmployeeCommandHandler(
        IEmployeeRepository employees,
        ITenantRoleRepository roles,
        ITenantRepository tenants,
        IPlanRepository plans,
        IActivationTokenRepository activationTokens,
        IUnitOfWork uow)
    {
        _employees = employees;
        _roles = roles;
        _tenants = tenants;
        _plans = plans;
        _activationTokens = activationTokens;
        _uow = uow;
    }

    public async Task<CreateEmployeeResult> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        if (await _employees.ExistsByEmailInTenantAsync(request.TenantId, request.Email, cancellationToken))
            throw new ConflictException($"E-mail '{request.Email}' já está em uso neste estabelecimento.");

        var role = await _roles.GetByIdAsync(request.TenantRoleId, cancellationToken)
            ?? throw new NotFoundException("TenantRole", request.TenantRoleId);

        if (role.TenantId != request.TenantId)
            throw new ForbiddenException();

        if (!role.IsActive)
            throw new ConflictException("O cargo selecionado está inativo.");

        if (role.IsOwnerRole)
            throw new ForbiddenException("Não é permitido criar funcionários com o cargo de Dono.");

        var tenant = await _tenants.GetByIdAsync(request.TenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenant), request.TenantId);

        var plan = await _plans.GetByIdAsync(tenant.PlanId, cancellationToken)
            ?? throw new NotFoundException(nameof(Plan), tenant.PlanId);

        var currentCount = await _employees.CountByTenantAsync(request.TenantId, cancellationToken);
        if (currentCount >= plan.MaxUsers)
            throw new ConflictException($"Limite de {plan.MaxUsers} funcionário(s) atingido pelo plano atual.");

        var employee = Employee.Create(request.TenantId, request.Name, request.Email, request.TenantRoleId);
        await _employees.AddAsync(employee, cancellationToken);

        var token = ActivationToken.Create(employee.Id, TimeSpan.FromHours(48));
        await _activationTokens.AddAsync(token, cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);
        return new CreateEmployeeResult(employee.Id, token.Token);
    }
}
