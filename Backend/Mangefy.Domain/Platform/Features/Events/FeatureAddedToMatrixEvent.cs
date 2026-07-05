using Mangefy.Domain.Common;

namespace Mangefy.Domain.Platform.Features.Events;

/// <summary>
/// Disparado quando o AdminSaas adiciona uma feature a uma combinação Plano × Tipo de Negócio.
/// Efeito imediato — tenants existentes ganham acesso sem carência.
/// </summary>
public sealed class FeatureAddedToMatrixEvent(
    Guid planFeatureSetId,
    Guid planId,
    Guid businessTypeId,
    string featureKey) : DomainEvent
{
    public Guid PlanFeatureSetId { get; } = planFeatureSetId;
    public Guid PlanId { get; } = planId;
    public Guid BusinessTypeId { get; } = businessTypeId;
    public string FeatureKey { get; } = featureKey;
}
