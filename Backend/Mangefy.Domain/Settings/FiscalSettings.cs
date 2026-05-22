using Mangefy.Domain.Common;

namespace Mangefy.Domain.Settings;

/// <summary>
/// Configurações fiscais do tenant.
/// NFC-e é opcional — implementação real via hub fiscal (Focus NFe / NFe.io) é trabalho futuro.
/// </summary>
public sealed class FiscalSettings : AggregateRoot
{
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Habilita a emissão de NFC-e ao fechar comandas. Requer CNPJ e certificado digital.
    /// </summary>
    public bool NfceEnabled { get; private set; }

    /// <summary>
    /// Emite NFC-e automaticamente ao fechar a comanda (se habilitado).
    /// Se false, o operador emite manualmente.
    /// </summary>
    public bool AutoEmitOnTabClose { get; private set; }

    // TODO: adicionar campos de certificado digital e configuração do hub fiscal
    // quando a integração com Focus NFe / NFe.io for implementada.
    public string? FiscalHubApiKey { get; private set; }
    public string? Cnpj { get; private set; }

    private FiscalSettings() { }

    public static FiscalSettings CreateDefault(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        return new FiscalSettings
        {
            TenantId = tenantId,
            NfceEnabled = false,
            AutoEmitOnTabClose = false
        };
    }

    public void EnableNfce(string cnpj, string fiscalHubApiKey, bool autoEmitOnTabClose = false)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            throw new DomainException("CNPJ é obrigatório para habilitar NFC-e.");

        if (string.IsNullOrWhiteSpace(fiscalHubApiKey))
            throw new DomainException("Chave de API do hub fiscal é obrigatória.");

        NfceEnabled = true;
        AutoEmitOnTabClose = autoEmitOnTabClose;
        Cnpj = cnpj.Trim();
        FiscalHubApiKey = fiscalHubApiKey.Trim();
        SetUpdatedAt();
    }

    public void DisableNfce()
    {
        NfceEnabled = false;
        AutoEmitOnTabClose = false;
        SetUpdatedAt();
    }

    public void SetAutoEmit(bool autoEmit)
    {
        if (!NfceEnabled)
            throw new DomainException("NFC-e não está habilitada.");

        AutoEmitOnTabClose = autoEmit;
        SetUpdatedAt();
    }
}
