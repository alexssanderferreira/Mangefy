using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Plans;
using Mangefy.Domain.Plans.Repositories;
using Mangefy.Domain.Roles;
using Mangefy.Domain.Roles.Repositories;
using Mangefy.Domain.Tenants;
using Mangefy.Domain.Tenants.Repositories;
using MediatR;

namespace Mangefy.Application.Tenants.Commands.ChangeTenantPlan;

public sealed class ChangeTenantPlanCommandHandler : IRequestHandler<ChangeTenantPlanCommand>
{
    private readonly ITenantRepository _tenants;
    private readonly IPlanRepository _plans;
    private readonly ITenantRoleRepository _roles;
    private readonly IUnitOfWork _uow;

    public ChangeTenantPlanCommandHandler(
        ITenantRepository tenants,
        IPlanRepository plans,
        ITenantRoleRepository roles,
        IUnitOfWork uow)
    {
        _tenants = tenants;
        _plans = plans;
        _roles = roles;
        _uow = uow;
    }

    public async Task Handle(ChangeTenantPlanCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenants.GetByIdAsync(request.TenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenant), request.TenantId);

        var currentPlan = await _plans.GetByIdAsync(tenant.PlanId, cancellationToken);

        var newPlan = await _plans.GetByIdAsync(request.NewPlanId, cancellationToken)
            ?? throw new NotFoundException(nameof(Plan), request.NewPlanId);

        if (newPlan.Status == Mangefy.Domain.Plans.PlanStatus.Inactive)
            throw new ConflictException("Não é possível migrar para um plano inativo.");

        // Em downgrade, desativar cargos customizados excedentes (determinístico: mais recentes primeiro)
        bool isDowngrade = currentPlan != null && newPlan.MaxCustomRoles < currentPlan.MaxCustomRoles;
        if (isDowngrade)
        {
            var allRoles = await _roles.GetByTenantAsync(request.TenantId, cancellationToken);
            var customRoles = allRoles
                .Where(r => !r.IsOwnerRole && !r.IsFromTemplate && r.IsActive)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            int excessCount = customRoles.Count - newPlan.MaxCustomRoles;
            for (int i = 0; i < excessCount && i < customRoles.Count; i++)
            {
                customRoles[i].DeactivateByPlanDowngrade();
                await _roles.UpdateAsync(customRoles[i], cancellationToken);
            }
        }

        tenant.ChangePlan(request.NewPlanId);
        await _tenants.UpdateAsync(tenant, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
