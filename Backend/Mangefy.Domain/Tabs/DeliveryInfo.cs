using Mangefy.Domain.Common;

namespace Mangefy.Domain.Tabs;

public sealed class DeliveryInfo : ValueObject
{
    public string RecipientName { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string? Complement { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? ExternalOrderRef { get; private set; }

    private DeliveryInfo() { }

    public static DeliveryInfo Create(
        string recipientName,
        string address,
        string? complement = null,
        string? phoneNumber = null,
        string? externalOrderRef = null)
    {
        if (string.IsNullOrWhiteSpace(recipientName))
            throw new DomainException("Nome do destinatário é obrigatório.");

        if (string.IsNullOrWhiteSpace(address))
            throw new DomainException("Endereço de entrega é obrigatório.");

        return new DeliveryInfo
        {
            RecipientName = recipientName.Trim(),
            Address = address.Trim(),
            Complement = complement?.Trim(),
            PhoneNumber = phoneNumber?.Trim(),
            ExternalOrderRef = externalOrderRef?.Trim()
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return RecipientName;
        yield return Address;
        yield return Complement ?? string.Empty;
        yield return PhoneNumber ?? string.Empty;
        yield return ExternalOrderRef ?? string.Empty;
    }
}
