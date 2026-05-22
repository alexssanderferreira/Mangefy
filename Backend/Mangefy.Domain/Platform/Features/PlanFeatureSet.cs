using Mangefy.Domain.Common;
using Mangefy.Domain.Platform.Features.Events;

namespace Mangefy.Domain.Platform.Features;

/// <summary>
/// Define quais features estão ativas para uma combinação específica de Plano + Tipo de Negócio.
/// Gerenciado exclusivamente pelo AdminSaas.
/// </summary>
public sealed class PlanFeatureSet : AggregateRoot
{
    public Guid PlanId { get; private set; }
    public Guid BusinessTypeId { get; private set; }

    private readonly List<string> _enabledFeatures = [];
    public IReadOnlyCollection<string> EnabledFeatures => _enabledFeatures.AsReadOnly();

    private PlanFeatureSet() { }

    public static PlanFeatureSet Create(Guid planId, Guid businessTypeId)
    {
        if (planId == Guid.Empty)
            throw new DomainException("PlanId inválido.");

        if (businessTypeId == Guid.Empty)
            throw new DomainException("BusinessTypeId inválido.");

        return new PlanFeatureSet
        {
            PlanId = planId,
            BusinessTypeId = businessTypeId
        };
    }

    /// <summary>
    /// Adiciona uma feature à combinação. Efeito imediato para tenants existentes.
    /// </summary>
    public void AddFeature(string featureKey)
    {
        if (!FeatureCatalog.IsValid(featureKey))
            throw new DomainException($"Feature '{featureKey}' não existe no catálogo.");

        if (_enabledFeatures.Contains(featureKey))
            return;

        _enabledFeatures.Add(featureKey);
        SetUpdatedAt();
        AddDomainEvent(new FeatureAddedToMatrixEvent(Id, PlanId, BusinessTypeId, featureKey));
    }

    /// <summary>
    /// Remove uma feature da combinação.
    /// Dispara evento para que a Application layer crie o FeatureGracePeriod nos tenants afetados.
    /// </summary>
    public void RemoveFeature(string featureKey, int gracePeriodDays = 30)
    {
        if (!_enabledFeatures.Contains(featureKey))
            return;

        if (gracePeriodDays < 0)
            throw new DomainException("Período de carência não pode ser negativo.");

        _enabledFeatures.Remove(featureKey);
        SetUpdatedAt();
        AddDomainEvent(new FeatureRemovedFromMatrixEvent(Id, PlanId, BusinessTypeId, featureKey, gracePeriodDays));
    }

    public bool HasFeature(string featureKey) => _enabledFeatures.Contains(featureKey);
}
