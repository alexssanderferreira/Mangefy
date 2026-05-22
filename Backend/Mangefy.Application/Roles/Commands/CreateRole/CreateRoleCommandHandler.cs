using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Platform.Features;
using Mangefy.Domain.Plans;
using Mangefy.Domain.Plans.Repositories;
using Mangefy.Domain.Tenants;
using Mangefy.Domain.Roles;
using Mangefy.Domain.Roles.Repositories;
using Mangefy.Domain.Tenants.Repositories;
using MediatR;

namespace Mangefy.Application.Roles.Commands.CreateRole;

public sealed class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Guid>
{
    private readonly ITenantRoleRepository _roles;
    private readonly ITenantRepository _tenants;
    private readonly IPlanRepository _plans;
    private readonly IFeatureGateService _featureGate;
    private readonly IUnitOfWork _uow;

    public CreateRoleCommandHandler(
        ITenantRoleRepository roles, ITenantRepository tenants,
        IPlanRepository plans, IFeatureGateService featureGate, IUnitOfWork uow)
    {
        _roles = roles;
        _tenants = tenants;
        _plans = plans;
        _featureGate = featureGate;
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        await _featureGate.RequireAsync(request.TenantId, FeatureCatalog.Roles.CustomRoles, cancellationToken);

        var tenant = await _tenants.GetByIdAsync(request.TenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenant), request.TenantId);

        var plan = await _plans.GetByIdAsync(tenant.PlanId, cancellationToken)
            ?? throw new NotFoundException(nameof(Plan), tenant.PlanId);

        if (plan.MaxCustomRoles == 0)
            throw new ForbiddenException("O plano atual não permite criar cargos customizados.");

        var existingRoles = await _roles.GetByTenantAsync(request.TenantId, cancellationToken);
        var customCount = existingRoles.Count(r => !r.IsFromTemplate && !r.IsOwnerRole);

        if (customCount >= plan.MaxCustomRoles)
            throw new ConflictException($"Limite de {plan.MaxCustomRoles} cargo(s) customizado(s) atingido.");

        if (await _roles.ExistsByNameAsync(request.TenantId, request.Name, cancellationToken))
            throw new ConflictException($"Já existe um cargo com o nome '{request.Name}'.");

        var role = TenantRole.Create(request.TenantId, request.Name, request.Description);

        if (request.Permissions.Any())
            role.SetPermissions(request.Permissions);

        await _roles.AddAsync(role, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return role.Id;
    }
}
