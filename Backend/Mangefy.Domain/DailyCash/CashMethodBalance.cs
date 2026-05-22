using Mangefy.Domain.Common;
using Mangefy.Domain.Tabs;

namespace Mangefy.Domain.DailyCash;

/// <summary>
/// Balanço de um método de pagamento no fechamento do caixa.
/// ExpectedAmount = calculado pelo sistema a partir das comandas fechadas.
/// CountedAmount  = contado fisicamente pelo operador.
/// </summary>
public sealed class CashMethodBalance : ValueObject
{
    public PaymentMethod Method { get; private set; }
    public decimal ExpectedAmount { get; private set; }
    public decimal CountedAmount { get; private set; }
    public decimal Difference => CountedAmount - ExpectedAmount;

    private CashMethodBalance() { }

    public static CashMethodBalance Create(PaymentMethod method, decimal expectedAmount, decimal countedAmount)
    {
        if (expectedAmount < 0)
            throw new DomainException("Valor esperado não pode ser negativo.");

        if (countedAmount < 0)
            throw new DomainException("Valor contado não pode ser negativo.");

        return new CashMethodBalance
        {
            Method = method,
            ExpectedAmount = Math.Round(expectedAmount, 2),
            CountedAmount = Math.Round(countedAmount, 2)
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Method;
        yield return ExpectedAmount;
        yield return CountedAmount;
    }
}
