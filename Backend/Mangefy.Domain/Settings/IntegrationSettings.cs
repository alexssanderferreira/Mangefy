using Mangefy.Domain.Common;

namespace Mangefy.Domain.Settings;

/// <summary>
/// Configurações de integrações externas do tenant.
/// Estrutura reservada — integrações reais (iFood, Rappi, etc.) são trabalho futuro.
/// O acesso a integrações é controlado pela feature features.delivery no PlanFeatureSet.
/// </summary>
public sealed class IntegrationSettings : AggregateRoot
{
    public Guid TenantId { get; private set; }

    // TODO: adicionar campos de integração com iFood, Rappi e outros
    // quando as APIs dos parceiros forem pesquisadas e implementadas.

    /// <summary>
    /// Indica se o tenant optou por habilitar integrações de delivery (requer features.delivery no plano).
    /// </summary>
    public bool DeliveryIntegrationEnabled { get; private set; }

    private IntegrationSettings() { }

    public static IntegrationSettings CreateDefault(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        return new IntegrationSettings
        {
            TenantId = tenantId,
            DeliveryIntegrationEnabled = false
        };
    }

    /// <summary>
    /// Habilita integrações de delivery. A Application layer deve verificar
    /// se o plano do tenant possui features.delivery antes de chamar este método.
    /// </summary>
    public void EnableDeliveryIntegration()
    {
        DeliveryIntegrationEnabled = true;
        SetUpdatedAt();
    }

    public void DisableDeliveryIntegration()
    {
        DeliveryIntegrationEnabled = false;
        SetUpdatedAt();
    }
}
