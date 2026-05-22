using Mangefy.Domain.Common;
using Mangefy.Domain.Roles;

namespace Mangefy.Domain.Platform.BusinessTypes;

/// <summary>
/// Template de cargo associado a um tipo de negócio.
/// Ao criar um tenant, cada template ativo vira uma cópia (TenantRole) independente.
/// </summary>
public sealed class RoleTemplate : Entity
{
    public Guid BusinessTypeId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<string> _permissions = [];
    public IReadOnlyCollection<string> Permissions => _permissions.AsReadOnly();

    private RoleTemplate() { }

    internal static RoleTemplate Create(Guid businessTypeId, string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do template de cargo não pode ser vazio.");

        return new RoleTemplate
        {
            BusinessTypeId = businessTypeId,
            Name = name.Trim(),
            Description = description?.Trim(),
            IsActive = true
        };
    }

    internal void UpdateInfo(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do template de cargo não pode ser vazio.");

        Name = name.Trim();
        Description = description?.Trim();
        SetUpdatedAt();
    }

    internal void SetPermissions(IEnumerable<string> permissions)
    {
        var list = permissions.ToList();
        var invalid = list.Where(p => !PermissionCatalog.IsValid(p)).ToList();

        if (invalid.Any())
            throw new DomainException($"Permissões inválidas: {string.Join(", ", invalid)}");

        _permissions.Clear();
        _permissions.AddRange(list.Distinct());
        SetUpdatedAt();
    }

    internal void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    internal void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }
}
