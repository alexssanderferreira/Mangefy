using Mangefy.Domain.Common;
using Mangefy.Domain.Common.ValueObjects;

namespace Mangefy.Domain.Tabs;

public sealed class Payment : Entity
{
    public Guid TabId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public PaymentMethod Method { get; private set; }
    public decimal ChangeGiven { get; private set; }
    public string? ExternalReference { get; private set; }  // NSU, código de autorização, etc.
    public DateTime PaidAt { get; private set; }

    private Payment() { }

    internal static Payment Create(
        Guid tabId,
        decimal amount,
        PaymentMethod method,
        decimal changeGiven = 0m,
        string? externalReference = null)
    {
        if (tabId == Guid.Empty)
            throw new DomainException("TabId inválido.");

        if (changeGiven < 0)
            throw new DomainException("Troco não pode ser negativo.");

        return new Payment
        {
            TabId = tabId,
            Amount = Money.Create(amount),
            Method = method,
            ChangeGiven = changeGiven,
            ExternalReference = externalReference?.Trim(),
            PaidAt = DateTime.UtcNow
        };
    }
}
