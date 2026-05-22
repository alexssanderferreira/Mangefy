using Mangefy.Domain.Common;

namespace Mangefy.Domain.Platform.Features;

/// <summary>
/// Período de carência concedido a um tenant quando uma feature é removida da sua combinação
/// Plano × Tipo de Negócio. O tenant mantém acesso até ExpiresAt, depois é bloqueado.
/// Criado pela Application layer ao processar o FeatureRemovedFromMatrixEvent.
/// </summary>
public sealed class FeatureGracePeriod : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public string FeatureKey { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? NotifiedAt { get; private set; }
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    private FeatureGracePeriod() { }

    public static FeatureGracePeriod Create(Guid tenantId, string featureKey, int gracePeriodDays)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        if (!FeatureCatalog.IsValid(featureKey))
            throw new DomainException($"Feature '{featureKey}' não existe no catálogo.");

        if (gracePeriodDays <= 0)
            throw new DomainException("Período de carência deve ser maior que zero.");

        return new FeatureGracePeriod
        {
            TenantId = tenantId,
            FeatureKey = featureKey,
            ExpiresAt = DateTime.UtcNow.AddDays(gracePeriodDays)
        };
    }

    /// <summary>
    /// Registra que o tenant foi notificado sobre a remoção da feature.
    /// </summary>
    public void MarkAsNotified()
    {
        NotifiedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    /// <summary>
    /// Verifica se o tenant ainda tem acesso à feature (dentro do período de carência).
    /// </summary>
    public bool IsActive() => !IsExpired;
}
