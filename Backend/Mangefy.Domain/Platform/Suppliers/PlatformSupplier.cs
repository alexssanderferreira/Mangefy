using Mangefy.Domain.Common;
using Mangefy.Domain.Common.ValueObjects;

namespace Mangefy.Domain.Platform.Suppliers;

/// <summary>
/// Fornecedor no catálogo global da plataforma, gerenciado pelo AdminSaas.
/// Tenants consultam este catálogo para conhecer e adicionar fornecedores à sua lista.
/// Dados somente leitura para o tenant — editáveis apenas pelo AdminSaas.
/// </summary>
public sealed class PlatformSupplier : AggregateRoot
{
    public string Name { get; private set; }
    public string? Cnpj { get; private set; }
    public Guid SupplierCategoryId { get; private set; }
    public string? Website { get; private set; }
    public Email? Email { get; private set; }
    public PhoneNumber? Phone { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public Address? Address { get; private set; }
    public string? BusinessHours { get; private set; }

    private PlatformSupplier() { }

    public static PlatformSupplier Create(
        string name,
        Guid supplierCategoryId,
        string? cnpj = null,
        string? website = null,
        string? email = null,
        string? phone = null,
        string? description = null,
        string? cep = null,
        string? logradouro = null,
        string? numero = null,
        string? bairro = null,
        string? cidade = null,
        string? uf = null,
        string? complemento = null,
        string? businessHours = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do fornecedor não pode ser vazio.");

        if (supplierCategoryId == Guid.Empty)
            throw new DomainException("Categoria do fornecedor inválida.");

        var supplier = new PlatformSupplier
        {
            Name = name.Trim(),
            Cnpj = cnpj?.Trim(),
            SupplierCategoryId = supplierCategoryId,
            Website = website?.Trim(),
            Email = email is not null ? Email.Create(email) : null,
            Phone = phone is not null ? PhoneNumber.Create(phone) : null,
            Description = description?.Trim(),
            BusinessHours = businessHours?.Trim(),
            IsActive = true
        };

        if (!string.IsNullOrWhiteSpace(cep) || !string.IsNullOrWhiteSpace(logradouro))
            supplier.Address = Address.Create(cep!, logradouro!, numero!, bairro!, cidade!, uf!, complemento);

        return supplier;
    }

    public void Update(
        string name,
        Guid supplierCategoryId,
        string? cnpj,
        string? website,
        string? email,
        string? phone,
        string? description,
        string? cep,
        string? logradouro,
        string? numero,
        string? bairro,
        string? cidade,
        string? uf,
        string? complemento,
        string? businessHours)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do fornecedor não pode ser vazio.");

        if (supplierCategoryId == Guid.Empty)
            throw new DomainException("Categoria do fornecedor inválida.");

        Name = name.Trim();
        SupplierCategoryId = supplierCategoryId;
        Cnpj = cnpj?.Trim();
        Website = website?.Trim();
        Email = email is not null ? Email.Create(email) : null;
        Phone = phone is not null ? PhoneNumber.Create(phone) : null;
        Description = description?.Trim();
        BusinessHours = businessHours?.Trim();

        Address = (!string.IsNullOrWhiteSpace(cep) || !string.IsNullOrWhiteSpace(logradouro))
            ? Address.Create(cep!, logradouro!, numero!, bairro!, cidade!, uf!, complemento)
            : null;

        SetUpdatedAt();
    }

    public void Deactivate() { IsActive = false; SetUpdatedAt(); }
    public void Activate() { IsActive = true; SetUpdatedAt(); }
}
