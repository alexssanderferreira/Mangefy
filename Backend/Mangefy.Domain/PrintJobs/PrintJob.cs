using Mangefy.Domain.Common;
using Mangefy.Domain.Menus;

namespace Mangefy.Domain.PrintJobs;

/// <summary>
/// Fila de impressão preparatória. Representa um documento a ser impresso
/// em alguma impressora do tenant. A integração com impressora física é futura.
/// </summary>
public sealed class PrintJob : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public MenuItemStation Station { get; private set; }
    public Guid? PrinterId { get; private set; }
    public PrintJobType Type { get; private set; }

    /// <summary>
    /// Payload serializado (JSON) com os dados a serem impressos.
    /// Referência a entidade: ex. { "tabId": "...", "orderId": "..." }
    /// </summary>
    public string Payload { get; private set; } = string.Empty;

    public PrintJobStatus Status { get; private set; }
    public int Attempts { get; private set; }
    public string? ReimpressionReason { get; private set; }

    private PrintJob() { }

    public static PrintJob Create(
        Guid tenantId,
        MenuItemStation station,
        PrintJobType type,
        string payload,
        Guid? createdByEmployeeId = null,
        Guid? printerId = null)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        if (string.IsNullOrWhiteSpace(payload))
            throw new DomainException("Payload do trabalho de impressão não pode ser vazio.");

        return new PrintJob
        {
            TenantId = tenantId,
            Station = station,
            PrinterId = printerId,
            Type = type,
            Payload = payload,
            Status = PrintJobStatus.Pending,
            Attempts = 0,
            CreatedByEmployeeId = createdByEmployeeId
        };
    }

    public static PrintJob Reprint(
        Guid tenantId,
        MenuItemStation station,
        string payload,
        string reason,
        Guid createdByEmployeeId,
        Guid? printerId = null)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Motivo da reimpressão é obrigatório.");

        var job = Create(tenantId, station, PrintJobType.Reprint, payload, createdByEmployeeId, printerId);
        job.ReimpressionReason = reason.Trim();
        return job;
    }

    public void MarkAsPrinted()
    {
        Attempts++;
        Status = PrintJobStatus.Printed;
        SetUpdatedAt();
    }

    public void MarkAsFailed()
    {
        Attempts++;
        if (Attempts >= 3)
            Status = PrintJobStatus.Failed;
        SetUpdatedAt();
    }

    public void Cancel()
    {
        if (Status == PrintJobStatus.Printed)
            throw new DomainException("Trabalho já impresso não pode ser cancelado.");

        Status = PrintJobStatus.Cancelled;
        SetUpdatedAt();
    }
}
