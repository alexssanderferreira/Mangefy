using Mangefy.Domain.Common;
using Mangefy.Domain.Common.ValueObjects;

namespace Mangefy.Domain.Menus;

/// <summary>
/// Entrada do histórico de alterações de preço de um item do cardápio.
/// Garante rastreabilidade sem afetar snapshots de preço em pedidos antigos.
/// </summary>
public sealed class PriceHistoryEntry : Entity
{
    public Money PreviousPrice { get; private set; } = null!;
    public Money NewPrice { get; private set; } = null!;
    public DateTime ChangedAt { get; private set; }
    public Guid? ChangedByEmployeeId { get; private set; }
    public string? Reason { get; private set; }

    private PriceHistoryEntry() { }

    internal static PriceHistoryEntry Create(
        decimal previousPrice,
        decimal newPrice,
        Guid? changedByEmployeeId,
        string? reason)
    {
        return new PriceHistoryEntry
        {
            PreviousPrice = Money.Create(previousPrice),
            NewPrice = Money.Create(newPrice),
            ChangedAt = DateTime.UtcNow,
            ChangedByEmployeeId = changedByEmployeeId,
            Reason = reason?.Trim()
        };
    }
}
