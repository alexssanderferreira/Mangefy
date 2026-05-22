using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Employees;
using Mangefy.Domain.Employees.Repositories;
using Mangefy.Domain.Owners.Repositories;
using Mangefy.Domain.Roles;
using Mangefy.Domain.Roles.Repositories;
using Mangefy.Domain.Tenants;
using Mangefy.Domain.Tenants.Repositories;
using MediatR;

namespace Mangefy.Application.Auth.Commands.SwitchTenant;

public sealed class SwitchTenantCommandHandler : IRequestHandler<SwitchTenantCommand, SwitchTenantResult>
{
    private readonly IEmployeeRepository _employees;
    private readonly IOwnerRepository _owners;
    private readonly ITenantRepository _tenants;
    private readonly ITenantRoleRepository _roles;
    private readonly ITokenService _tokenService;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _uow;

    public SwitchTenantCommandHandler(
        IEmployeeRepository employees,
        IOwnerRepository owners,
        ITenantRepository tenants,
        ITenantRoleRepository roles,
        ITokenService tokenService,
        ICurrentUser currentUser,
        IUnitOfWork uow)
    {
        _employees = employees;
        _owners = owners;
        _tenants = tenants;
        _roles = roles;
        _tokenService = tokenService;
        _currentUser = currentUser;
        _uow = uow;
    }

    public async Task<SwitchTenantResult> Handle(SwitchTenantCommand request, CancellationToken cancellationToken)
    {
        var targetTenant = await _tenants.GetBySlugAsync(request.TargetTenantSlug, cancellationToken)
            ?? throw new ForbiddenException("Estabelecimento não encontrado.");

        if (targetTenant.Status == TenantStatus.Suspended)
            throw new ForbiddenException("Este estabelecimento está suspenso.");

        if (targetTenant.Status == TenantStatus.Cancelled)
            throw new ForbiddenException("Este estabelecimento foi cancelado.");

        // Sessão de Owner
        if (_currentUser.OwnerId.HasValue)
            return await SwitchAsOwner(_currentUser.OwnerId.Value, targetTenant, cancellationToken);

        // Sessão de Employee
        var currentEmployeeId = _currentUser.EmployeeId
            ?? throw new ForbiddenException("Não autenticado.");

        return await SwitchAsEmployee(currentEmployeeId, targetTenant, cancellationToken);
    }

    private async Task<SwitchTenantResult> SwitchAsOwner(
        Guid ownerId,
        Domain.Tenants.Tenant targetTenant,
        CancellationToken ct)
    {
        var owner = await _owners.GetByIdAsync(ownerId, ct)
            ?? throw new ForbiddenException("Dono não encontrado.");

        if (owner.Status == Domain.Owners.OwnerStatus.Inactive)
            throw new ForbiddenException("Conta de dono inativa. Contate o suporte.");

        if (owner.Status == Domain.Owners.OwnerStatus.PendingActivation)
            throw new ForbiddenException("Conta ainda não ativada. Verifique o e-mail de ativação.");

        if (targetTenant.OwnerId != owner.Id)
            throw new ForbiddenException("Você não tem acesso a este estabelecimento.");

        var permissions = PermissionCatalog.All.ToList();
        var token = _tokenService.GenerateOwnerTenantToken(owner.Id, targetTenant.Id, owner.Email.Value, permissions);

        owner.RecordLogin();
        await _owners.UpdateAsync(owner, ct);
        await _uow.SaveChangesAsync(ct);

        return new SwitchTenantResult(token.AccessToken, token.ExpiresAt,
            EmployeeId: null, OwnerId: owner.Id,
            targetTenant.Id, owner.Name, IsOwner: true, permissions);
    }

    private async Task<SwitchTenantResult> SwitchAsEmployee(
        Guid currentEmployeeId,
        Domain.Tenants.Tenant targetTenant,
        CancellationToken ct)
    {
        var currentEmployee = await _employees.GetByIdAsync(currentEmployeeId, ct)
            ?? throw new ForbiddenException("Usuário não encontrado.");

        var targetEmployee = await _employees.GetByEmailInTenantAsync(targetTenant.Id, currentEmployee.Email.Value, ct)
            ?? throw new ForbiddenException("Você não tem acesso a este estabelecimento.");

        if (targetEmployee.Status == EmployeeStatus.Inactive)
            throw new ForbiddenException("Funcionário inativo neste estabelecimento.");

        if (targetEmployee.Status == EmployeeStatus.PendingActivation)
            throw new ForbiddenException("Conta ainda não ativada neste estabelecimento.");

        var role = await _roles.GetByIdAsync(targetEmployee.TenantRoleId, ct)
            ?? throw new ForbiddenException("Cargo não encontrado.");

        if (!role.IsActive && !role.IsOwnerRole)
            throw new ForbiddenException("Cargo inativo. Contate o responsável pelo estabelecimento.");

        var permissions = role.IsOwnerRole
            ? PermissionCatalog.All.ToList()
            : role.Permissions.ToList();

        var token = _tokenService.GenerateToken(targetEmployee.Id, targetEmployee.TenantId, targetEmployee.Email.Value, permissions);

        targetEmployee.RecordLogin();
        await _employees.UpdateAsync(targetEmployee, ct);
        await _uow.SaveChangesAsync(ct);

        return new SwitchTenantResult(token.AccessToken, token.ExpiresAt,
            EmployeeId: targetEmployee.Id, OwnerId: null,
            targetTenant.Id, targetEmployee.Name, role.IsOwnerRole, permissions);
    }
}
