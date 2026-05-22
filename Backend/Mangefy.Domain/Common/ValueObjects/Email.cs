using System.Text.RegularExpressions;

namespace Mangefy.Domain.Common.ValueObjects;

public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("E-mail não pode ser vazio.");

        if (!EmailRegex.IsMatch(value))
            throw new DomainException($"'{value}' não é um e-mail válido.");

        return new Email(value.ToLowerInvariant().Trim());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
