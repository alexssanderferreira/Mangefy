using Mangefy.Domain.Common;
using Mangefy.Domain.Common.ValueObjects;

namespace Mangefy.Domain.DailyCash;

/// <summary>
/// Suprimento — entrada de dinheiro no caixa durante o turno (reforço de troco etc.).
/// </summary>
public sealed class CashSupply : Entity
{
    public Money Amount { get; private set; } = null!;
    public string Reason { get; private set; } = string.Empty;
    public Guid EmployeeId { get; private set; }

    private CashSupply() { }

    internal static CashSupply Create(decimal amount, string reason, Guid employeeId)
    {
        if (amount <= 0)
            throw new DomainException("Valor do suprimento deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Motivo do suprimento não pode ser vazio.");

        return new CashSupply
        {
            Amount = Money.Create(amount),
            Reason = reason.Trim(),
            EmployeeId = employeeId
        };
    }
}
