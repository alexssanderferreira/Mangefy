using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Plans.Repositories;
using Mangefy.Domain.Roles.Repositories;
using Mangefy.Domain.Tenants.Events;
using MediatR;

namespace Mangefy.Application.Tenants.EventHandlers;

/// <summary>
/// Em downgrade de plano, desativa cargos customizados excedentes (mais recentes primeiro).
/// O handler é chamado após ChangeTenantPlanCommandHandler já ter feito a desativação inline,
/// mas existe como safety net para desativações disparadas por outros fluxos (ex: plano expirado).
/// </summary>
public sealed class TenantPlanChangedEventHandler : INotificationHandler<TenantPlanChangedEvent>
{
    private readonly IPlanRepository _plans;
    private readonly ITenantRoleRepository _roles;
    private readonly IUnitOfWork _uow;

    public TenantPlanChangedEventHandler(
        IPlanRepository plans,
        ITenantRoleRepository roles,
        IUnitOfWork uow)
    {
        _plans = plans;
        _roles = roles;
        _uow = uow;
    }

    public async Task Handle(TenantPlanChangedEvent notification, CancellationToken cancellationToken)
    {
        var newPlan = await _plans.GetByIdAsync(notification.NewPlanId, cancellationToken);
        if (newPlan is null) return;

        var previousPlan = await _plans.GetByIdAsync(notification.PreviousPlanId, cancellationToken);
        if (previousPlan is null) return;

        bool isDowngrade = newPlan.MaxCustomRoles < previousPlan.MaxCustomRoles;
        if (!isDowngrade) return;

        var allRoles = await _roles.GetByTenantAsync(notification.TenantId, cancellationToken);
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
}
