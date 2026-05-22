using Mangefy.Domain.Common;
using Mangefy.Domain.Platform.Subscriptions.Events;

namespace Mangefy.Domain.Platform.Subscriptions;

/// <summary>
/// Assinatura de um tenant na plataforma Mangefy.
/// AdminSaas registra faturas e confirma pagamentos manualmente.
/// Integração com processadora de pagamento é trabalho futuro.
/// </summary>
public sealed class Subscription : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid PlanId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly NextDueDate { get; private set; }

    private readonly List<Invoice> _invoices = [];
    public IReadOnlyList<Invoice> Invoices => _invoices.AsReadOnly();

    private Subscription() { }

    public static Subscription Create(Guid tenantId, Guid planId, DateOnly startDate, DateOnly nextDueDate)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        if (planId == Guid.Empty)
            throw new DomainException("PlanId inválido.");

        if (nextDueDate <= startDate)
            throw new DomainException("Próximo vencimento deve ser após a data de início.");

        return new Subscription
        {
            TenantId = tenantId,
            PlanId = planId,
            StartDate = startDate,
            NextDueDate = nextDueDate
        };
    }

    /// <summary>
    /// Gera uma nova fatura para o próximo período. Chamado pelo AdminSaas ou por job agendado.
    /// </summary>
    public Invoice GenerateInvoice(decimal amount, DateOnly dueDate)
    {
        var invoice = Invoice.Create(TenantId, amount, dueDate);
        _invoices.Add(invoice);
        SetUpdatedAt();
        return invoice;
    }

    /// <summary>
    /// AdminSaas confirma que o pagamento foi recebido.
    /// </summary>
    public void ConfirmPayment(Guid invoiceId, DateOnly paidAt,
        DateOnly nextDueDate, string? paymentReference = null, string? notes = null)
    {
        var invoice = GetInvoiceOrThrow(invoiceId);
        invoice.MarkAsPaid(paidAt, paymentReference, notes);

        NextDueDate = nextDueDate;
        SetUpdatedAt();

        AddDomainEvent(new InvoicePaidEvent(Id, TenantId, invoiceId, invoice.Amount.Amount, paidAt));
    }

    /// <summary>
    /// Marca faturas pendentes como atrasadas. Chamado por job agendado diariamente.
    /// </summary>
    public void MarkOverdueInvoices(DateOnly today)
    {
        var overdue = _invoices
            .Where(i => i.Status == InvoiceStatus.Pending && i.DueDate < today)
            .ToList();

        foreach (var invoice in overdue)
        {
            invoice.MarkAsOverdue();
            AddDomainEvent(new InvoiceOverdueEvent(Id, TenantId, invoice.Id, invoice.DueDate));
        }

        if (overdue.Count > 0)
            SetUpdatedAt();
    }

    public void ChangePlan(Guid newPlanId)
    {
        if (newPlanId == Guid.Empty)
            throw new DomainException("PlanId inválido.");

        PlanId = newPlanId;
        SetUpdatedAt();
    }

    public bool HasOverdueInvoices() =>
        _invoices.Any(i => i.Status == InvoiceStatus.Overdue);

    public Invoice? GetLatestInvoice() =>
        _invoices.OrderByDescending(i => i.DueDate).FirstOrDefault();

    private Invoice GetInvoiceOrThrow(Guid invoiceId) =>
        _invoices.FirstOrDefault(i => i.Id == invoiceId)
        ?? throw new DomainException("Fatura não encontrada.");
}
