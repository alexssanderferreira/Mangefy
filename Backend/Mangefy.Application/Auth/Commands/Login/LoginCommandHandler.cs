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

namespace Mangefy.Application.Auth.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IEmployeeRepository _employees;
    private readonly IOwnerRepository _owners;
    private readonly ITenantRepository _tenants;
    private readonly ITenantRoleRepository _roles;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _uow;

    public LoginCommandHandler(
        IEmployeeRepository employees,
        IOwnerRepository owners,
        ITenantRepository tenants,
        ITenantRoleRepository roles,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUnitOfWork uow)
    {
        _employees = employees;
        _owners = owners;
        _tenants = tenants;
        _roles = roles;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _uow = uow;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenants.GetBySlugAsync(request.TenantSlug, cancellationToken)
            ?? throw new ForbiddenException("Credenciais inválidas.");

        if (tenant.Status == TenantStatus.Suspended)
            throw new ForbiddenException("Este estabelecimento está suspenso. Contate o suporte.");

        if (tenant.Status == TenantStatus.Cancelled)
            throw new ForbiddenException("Este estabelecimento foi cancelado.");

        // Tenta login como Employee primeiro
        var employee = await _employees.GetByEmailInTenantAsync(tenant.Id, request.Email, cancellationToken);
        if (employee is not null)
            return await LoginAsEmployee(employee, tenant, request.Password, cancellationToken);

        // Fallback: tenta login como Owner do tenant
        var owner = await _owners.GetByEmailAsync(request.Email, cancellationToken);
        if (owner is not null && tenant.OwnerId == owner.Id)
            return await LoginAsOwner(owner, tenant, request.Password, cancellationToken);

        throw new ForbiddenException("Credenciais inválidas.");
    }

    private async Task<LoginResult> LoginAsEmployee(
        Domain.Employees.Employee employee,
        Domain.Tenants.Tenant tenant,
        string password,
        CancellationToken ct)
    {
        if (employee.Status == EmployeeStatus.Inactive)
            throw new ForbiddenException("Funcionário inativo. Contate o responsável pelo estabelecimento.");

        if (employee.Status == EmployeeStatus.PendingActivation)
            throw new ForbiddenException("Conta ainda não ativada. Verifique o e-mail de primeiro acesso.");

        if (employee.PasswordHash is null || !_passwordHasher.Verify(password, employee.PasswordHash))
            throw new ForbiddenException("Credenciais inválidas.");

        var role = await _roles.GetByIdAsync(employee.TenantRoleId, ct)
            ?? throw new ForbiddenException("Cargo não encontrado.");

        if (!role.IsActive && !role.IsOwnerRole)
            throw new ForbiddenException("Cargo inativo. Contate o responsável pelo estabelecimento.");

        var permissions = role.IsOwnerRole
            ? PermissionCatalog.All.ToList()
            : role.Permissions.ToList();

        var token = _tokenService.GenerateToken(employee.Id, employee.TenantId, employee.Email.Value, permissions);

        employee.RecordLogin();
        await _employees.UpdateAsync(employee, ct);
        await _uow.SaveChangesAsync(ct);

        return new LoginResult(token.AccessToken, token.ExpiresAt,
            EmployeeId: employee.Id, OwnerId: null,
            tenant.Id, employee.Name, role.IsOwnerRole, permissions);
    }

    private async Task<LoginResult> LoginAsOwner(
        Domain.Owners.Owner owner,
        Domain.Tenants.Tenant tenant,
        string password,
        CancellationToken ct)
    {
        if (owner.Status == Domain.Owners.OwnerStatus.Inactive)
            throw new ForbiddenException("Conta de dono inativa. Contate o suporte.");

        if (owner.Status == Domain.Owners.OwnerStatus.PendingActivation)
            throw new ForbiddenException("Conta ainda não ativada. Verifique o e-mail de ativação.");

        if (owner.PasswordHash is null || !_passwordHasher.Verify(password, owner.PasswordHash))
            throw new ForbiddenException("Credenciais inválidas.");

        var permissions = PermissionCatalog.All.ToList();
        var token = _tokenService.GenerateOwnerTenantToken(owner.Id, tenant.Id, owner.Email.Value, permissions);

        owner.RecordLogin();
        await _owners.UpdateAsync(owner, ct);
        await _uow.SaveChangesAsync(ct);

        return new LoginResult(token.AccessToken, token.ExpiresAt,
            EmployeeId: null, OwnerId: owner.Id,
            tenant.Id, owner.Name, IsOwner: true, permissions);
    }
}
