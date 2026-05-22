using Mangefy.Domain.Common;
using Mangefy.Domain.Common.ValueObjects;

namespace Mangefy.Domain.Stock;

/// <summary>
/// Fornecedor vinculado ao tenant.
/// PlatformSupplierId preenchido = originado do catálogo da plataforma (dados do catálogo são somente leitura).
/// PlatformSupplierId nulo = cadastrado manualmente pelo Owner (totalmente editável).
/// </summary>
public sealed class Supplier : AggregateRoot
{
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Referência ao PlatformSupplier de origem. Nulo = fornecedor manual do tenant.
    /// </summary>
    public Guid? PlatformSupplierId { get; private set; }

    public string Name { get; private set; }
    public string? Cnpj { get; private set; }
    public Guid SupplierCategoryId { get; private set; }
    public string? Website { get; private set; }
    public Email? Email { get; private set; }
    public PhoneNumber? Phone { get; private set; }

    /// <summary>
    /// Contato do representante comercial — campo exclusivo do tenant.
    /// </summary>
    public string? RepresentativeName { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }

    private Supplier() { }

    /// <summary>
    /// Adiciona um fornecedor do catálogo da plataforma à lista do tenant.
    /// Os dados principais são copiados do PlatformSupplier para exibição;
    /// o tenant só edita campos exclusivos (representante, notas).
    /// </summary>
    public static Supplier AddFromPlatform(
        Guid tenantId,
        Guid platformSupplierId,
        string name,
        Guid supplierCategoryId,
        string? cnpj,
        string? website,
        string? email,
        string? phone)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        if (platformSupplierId == Guid.Empty)
            throw new DomainException("PlatformSupplierId inválido.");

        return new Supplier
        {
            TenantId = tenantId,
            PlatformSupplierId = platformSupplierId,
            Name = name,
            Cnpj = cnpj,
            SupplierCategoryId = supplierCategoryId,
            Website = website,
            Email = email is not null ? Email.Create(email) : null,
            Phone = phone is not null ? PhoneNumber.Create(phone) : null,
            IsActive = true
        };
    }

    /// <summary>
    /// Cria um fornecedor manual, cadastrado diretamente pelo Owner.
    /// </summary>
    public static Supplier CreateManual(
        Guid tenantId,
        string name,
        Guid supplierCategoryId,
        string? cnpj = null,
        string? website = null,
        string? email = null,
        string? phone = null,
        string? representativeName = null,
        string? notes = null)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do fornecedor não pode ser vazio.");

        if (supplierCategoryId == Guid.Empty)
            throw new DomainException("Categoria do fornecedor inválida.");

        return new Supplier
        {
            TenantId = tenantId,
            PlatformSupplierId = null,
            Name = name.Trim(),
            Cnpj = cnpj?.Trim(),
            SupplierCategoryId = supplierCategoryId,
            Website = website?.Trim(),
            Email = email is not null ? Email.Create(email) : null,
            Phone = phone is not null ? PhoneNumber.Create(phone) : null,
            RepresentativeName = representativeName?.Trim(),
            Notes = notes?.Trim(),
            IsActive = true
        };
    }

    public void UpdateManualInfo(
        string name,
        Guid supplierCategoryId,
        string? cnpj,
        string? website,
        string? email,
        string? phone,
        string? representativeName,
        string? notes)
    {
        if (IsFromPlatform())
            throw new DomainException("Fornecedores do catálogo da plataforma não podem ser editados pelo tenant.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do fornecedor não pode ser vazio.");

        if (supplierCategoryId == Guid.Empty)
            throw new DomainException("Categoria do fornecedor inválida.");

        Name = name.Trim();
        Cnpj = cnpj?.Trim();
        SupplierCategoryId = supplierCategoryId;
        Website = website?.Trim();
        Email = email is not null ? Email.Create(email) : null;
        Phone = phone is not null ? PhoneNumber.Create(phone) : null;
        RepresentativeName = representativeName?.Trim();
        Notes = notes?.Trim();
        SetUpdatedAt();
    }

    public void UpdateRepresentativeInfo(string? representativeName, string? notes)
    {
        RepresentativeName = representativeName?.Trim();
        Notes = notes?.Trim();
        SetUpdatedAt();
    }

    public void Deactivate() { IsActive = false; SetUpdatedAt(); }
    public void Activate() { IsActive = true; SetUpdatedAt(); }

    public bool IsFromPlatform() => PlatformSupplierId.HasValue;
}
