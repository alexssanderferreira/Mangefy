using System.Text.RegularExpressions;

namespace Mangefy.Domain.Common.ValueObjects;

public sealed class PhoneNumber : ValueObject
{
    private static readonly Regex PhoneRegex = new(@"^\+?[1-9]\d{7,14}$", RegexOptions.Compiled);

    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Telefone não pode ser vazio.");

        var digits = Regex.Replace(value, @"[\s\-\(\)]", "");

        if (!PhoneRegex.IsMatch(digits))
            throw new DomainException($"'{value}' não é um número de telefone válido.");

        return new PhoneNumber(digits);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
