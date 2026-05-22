using Mangefy.Domain.Common;

namespace Mangefy.Domain.Platform.SupplierCategories;

/// <summary>
/// Ramo de atuação de fornecedores.
/// Categorias globais (TenantId = null) são gerenciadas pelo AdminSaas.
/// Categorias com TenantId são exclusivas do tenant que as criou.
/// </summary>
public sealed class SupplierCategory : AggregateRoot
{
    public string Name { get; private set; }
    public string? Description { get; private set; }

    /// <summary>
    /// Nulo = categoria global da plataforma. Preenchido = exclusiva do tenant.
    /// </summary>
    public Guid? TenantId { get; private set; }

    public bool IsActive { get; private set; }

    private SupplierCategory() { }

    public static SupplierCategory CreateGlobal(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da categoria não pode ser vazio.");

        return new SupplierCategory
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            TenantId = null,
            IsActive = true
        };
    }

    public static SupplierCategory CreateForTenant(Guid tenantId, string name, string? description = null)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da categoria não pode ser vazio.");

        return new SupplierCategory
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            TenantId = tenantId,
            IsActive = true
        };
    }

    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da categoria não pode ser vazio.");

        Name = name.Trim();
        Description = description?.Trim();
        SetUpdatedAt();
    }

    public void Deactivate() { IsActive = false; SetUpdatedAt(); }
    public void Activate() { IsActive = true; SetUpdatedAt(); }

    public bool IsGlobal() => TenantId is null;
}
