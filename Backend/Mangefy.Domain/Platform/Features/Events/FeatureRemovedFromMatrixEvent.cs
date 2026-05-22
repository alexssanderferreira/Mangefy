using Mangefy.Domain.Common;

namespace Mangefy.Domain.Platform.Features.Events;

/// <summary>
/// Disparado quando o AdminSaas remove uma feature de uma combinação Plano × Tipo de Negócio.
/// A Application layer usa este evento para criar os FeatureGracePeriod dos tenants afetados.
/// </summary>
public sealed class FeatureRemovedFromMatrixEvent(
    Guid planFeatureSetId,
    Guid planId,
    Guid businessTypeId,
    string featureKey,
    int gracePeriodDays) : DomainEvent
{
    public Guid PlanFeatureSetId { get; } = planFeatureSetId;
    public Guid PlanId { get; } = planId;
    public Guid BusinessTypeId { get; } = businessTypeId;
    public string FeatureKey { get; } = featureKey;
    public int GracePeriodDays { get; } = gracePeriodDays;
}
