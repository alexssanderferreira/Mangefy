using Mangefy.Domain.Common;
using Mangefy.Domain.Common.ValueObjects;

namespace Mangefy.Domain.Platform.Subscriptions;

public sealed class Invoice : Entity
{
    public Guid TenantId { get; private set; }
    public Money Amount { get; private set; }
    public DateOnly DueDate { get; private set; }
    public DateOnly? PaidAt { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public string? Notes { get; private set; }

    /// <summary>
    /// Referência externa (ex: número do boleto, código da transferência).
    /// </summary>
    public string? PaymentReference { get; private set; }

    private Invoice() { }

    internal static Invoice Create(Guid tenantId, decimal amount, DateOnly dueDate)
    {
        if (amount <= 0)
            throw new DomainException("Valor da fatura deve ser maior que zero.");

        return new Invoice
        {
            TenantId = tenantId,
            Amount = Money.Create(amount),
            DueDate = dueDate,
            Status = InvoiceStatus.Pending
        };
    }

    internal void MarkAsPaid(DateOnly paidAt, string? paymentReference = null, string? notes = null)
    {
        if (Status == InvoiceStatus.Paid)
            throw new DomainException("Fatura já está paga.");

        Status = InvoiceStatus.Paid;
        PaidAt = paidAt;
        PaymentReference = paymentReference?.Trim();
        Notes = notes?.Trim();
        SetUpdatedAt();
    }

    internal void MarkAsOverdue()
    {
        if (Status != InvoiceStatus.Pending)
            throw new DomainException("Apenas faturas pendentes podem ser marcadas como atrasadas.");

        Status = InvoiceStatus.Overdue;
        SetUpdatedAt();
    }

    internal void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
        SetUpdatedAt();
    }
}
