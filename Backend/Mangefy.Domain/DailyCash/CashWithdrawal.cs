using Mangefy.Domain.Common;
using Mangefy.Domain.Common.ValueObjects;

namespace Mangefy.Domain.DailyCash;

/// <summary>
/// Sangria — retirada de dinheiro do caixa durante o turno.
/// </summary>
public sealed class CashWithdrawal : Entity
{
    public Money Amount { get; private set; }
    public string Reason { get; private set; }
    public Guid EmployeeId { get; private set; }

    private CashWithdrawal() { }

    internal static CashWithdrawal Create(decimal amount, string reason, Guid employeeId)
    {
        if (amount <= 0)
            throw new DomainException("Valor da sangria deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Motivo da sangria não pode ser vazio.");

        return new CashWithdrawal
        {
            Amount = Money.Create(amount),
            Reason = reason.Trim(),
            EmployeeId = employeeId
        };
    }
}
