using Mangefy.Domain.Common;

namespace Mangefy.Domain.Tenants.Events;

/// <summary>
/// Disparado quando o plano do tenant muda (upgrade ou downgrade).
/// A Application layer usa este evento para verificar cargos customizados
/// que excedam o novo limite e desativá-los.
/// </summary>
public sealed class TenantPlanChangedEvent(Guid tenantId, Guid previousPlanId, Guid newPlanId) : DomainEvent
{
    public Guid TenantId { get; } = tenantId;
    public Guid PreviousPlanId { get; } = previousPlanId;
    public Guid NewPlanId { get; } = newPlanId;
}
