using Mangefy.Domain.Common;
using Mangefy.Domain.Common.ValueObjects;

namespace Mangefy.Domain.Owners;

public sealed class Owner : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public string? PasswordHash { get; private set; }
    public OwnerStatus Status { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    public PhoneNumber? Phone { get; private set; }
    public OwnerDocumentType? DocumentType { get; private set; }
    public string? DocumentNumber { get; private set; }
    public Address? Address { get; private set; }
    public string? Notes { get; private set; }

    private Owner() { }

    public static Owner Create(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do dono não pode ser vazio.");

        return new Owner
        {
            Name = name.Trim(),
            Email = Email.Create(email),
            Status = OwnerStatus.PendingActivation
        };
    }

    public void Activate()
    {
        if (Status == OwnerStatus.Inactive)
            throw new DomainException("Owner inativo não pode ser ativado diretamente. Use Reactivate.");

        Status = OwnerStatus.Active;
        SetUpdatedAt();
    }

    public void Reactivate()
    {
        Status = OwnerStatus.Active;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        Status = OwnerStatus.Inactive;
        SetUpdatedAt();
    }

    public void SetPassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Hash da senha não pode ser vazio.");

        PasswordHash = passwordHash;
        SetUpdatedAt();
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome não pode ser vazio.");

        Name = name.Trim();
        SetUpdatedAt();
    }

    public void ChangeEmail(string newEmail)
    {
        var email = Email.Create(newEmail);
        if (Email.Value.Equals(email.Value, StringComparison.OrdinalIgnoreCase))
            return;
        Email = email;
        SetUpdatedAt();
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void UpdateContactInfo(string? phone, OwnerDocumentType? documentType, string? documentNumber, string? notes)
    {
        Phone = string.IsNullOrWhiteSpace(phone) ? null : PhoneNumber.Create(phone);

        if (documentType.HasValue && !string.IsNullOrWhiteSpace(documentNumber))
        {
            var digits = new string(documentNumber.Where(char.IsDigit).ToArray());
            var expectedLen = documentType.Value == OwnerDocumentType.CPF ? 11 : 14;
            if (digits.Length != expectedLen)
                throw new DomainException($"{documentType.Value} inválido — esperado {expectedLen} dígitos.");
            DocumentType = documentType;
            DocumentNumber = digits;
        }
        else
        {
            DocumentType = null;
            DocumentNumber = null;
        }

        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        SetUpdatedAt();
    }

    public void SetAddress(string? cep, string? logradouro, string? numero,
        string? bairro, string? cidade, string? uf, string? complemento = null)
    {
        if (string.IsNullOrWhiteSpace(cep) && string.IsNullOrWhiteSpace(logradouro))
        {
            Address = null;
        }
        else
        {
            Address = Address.Create(cep!, logradouro!, numero!, bairro!, cidade!, uf!, complemento);
        }
        SetUpdatedAt();
    }
}
