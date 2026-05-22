namespace Mangefy.Domain.Common.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = "BRL")
    {
        if (amount < 0)
            throw new DomainException("Valor monetário não pode ser negativo.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Moeda não pode ser vazia.");

        return new Money(Math.Round(amount, 2), currency.ToUpperInvariant());
    }

    public static Money Zero(string currency = "BRL") => new(0, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Multiply(int quantity)
    {
        if (quantity < 0)
            throw new DomainException("Quantidade não pode ser negativa.");
        return new Money(Amount * quantity, Currency);
    }

    public Money Subtract(decimal amount)
    {
        if (amount < 0)
            throw new DomainException("Valor a subtrair não pode ser negativo.");
        var result = Amount - amount;
        if (result < 0)
            throw new DomainException("Resultado monetário não pode ser negativo.");
        return new Money(Math.Round(result, 2), Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException($"Não é possível operar moedas diferentes: {Currency} e {other.Currency}.");
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Currency} {Amount:F2}";
}
