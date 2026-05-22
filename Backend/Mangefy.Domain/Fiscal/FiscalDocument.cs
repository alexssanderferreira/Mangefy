using Mangefy.Domain.Common;
using Mangefy.Domain.Common.ValueObjects;

namespace Mangefy.Domain.Fiscal;

/// <summary>
/// Documento fiscal preparatório — registra intenção de emissão sem integração externa.
/// A integração com hub fiscal (Focus NFe / NFe.io) é trabalho futuro.
/// </summary>
public sealed class FiscalDocument : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid TabId { get; private set; }
    public FiscalDocumentType Type { get; private set; }
    public FiscalDocumentStatus Status { get; private set; }
    public FiscalEnvironment Environment { get; private set; }
    public Money TotalAmount { get; private set; } = null!;
    public string? AccessKey { get; private set; }     // chave de acesso após emissão
    public string? Protocol { get; private set; }      // protocolo de autorização
    public string? RejectReason { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime? IssuedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    private FiscalDocument() { }

    public static FiscalDocument CreatePending(
        Guid tenantId,
        Guid tabId,
        FiscalDocumentType type,
        FiscalEnvironment environment,
        decimal totalAmount)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        if (tabId == Guid.Empty)
            throw new DomainException("TabId inválido.");

        if (totalAmount <= 0)
            throw new DomainException("Valor do documento fiscal deve ser maior que zero.");

        return new FiscalDocument
        {
            TenantId = tenantId,
            TabId = tabId,
            Type = type,
            Status = FiscalDocumentStatus.Pending,
            Environment = environment,
            TotalAmount = Money.Create(totalAmount)
        };
    }

    public void MarkAsIssued(string accessKey, string protocol)
    {
        if (Status != FiscalDocumentStatus.Pending && Status != FiscalDocumentStatus.Contingency)
            throw new DomainException("Apenas documentos pendentes ou em contingência podem ser emitidos.");

        if (string.IsNullOrWhiteSpace(accessKey))
            throw new DomainException("Chave de acesso é obrigatória.");

        if (string.IsNullOrWhiteSpace(protocol))
            throw new DomainException("Protocolo de autorização é obrigatório.");

        AccessKey = accessKey.Trim();
        Protocol = protocol.Trim();
        Status = FiscalDocumentStatus.Issued;
        IssuedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void MarkAsRejected(string reason)
    {
        if (Status != FiscalDocumentStatus.Pending)
            throw new DomainException("Apenas documentos pendentes podem ser rejeitados.");

        RejectReason = reason?.Trim();
        Status = FiscalDocumentStatus.Rejected;
        SetUpdatedAt();
    }

    public void MarkAsContingency()
    {
        if (Status != FiscalDocumentStatus.Pending)
            throw new DomainException("Apenas documentos pendentes podem entrar em contingência.");

        Status = FiscalDocumentStatus.Contingency;
        SetUpdatedAt();
    }

    public void Cancel(string reason)
    {
        if (Status != FiscalDocumentStatus.Issued)
            throw new DomainException("Apenas documentos emitidos podem ser cancelados.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Motivo do cancelamento é obrigatório.");

        CancellationReason = reason.Trim();
        Status = FiscalDocumentStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}
