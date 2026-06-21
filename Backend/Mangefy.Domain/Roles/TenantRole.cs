using Mangefy.Domain.Common;
using Mangefy.Domain.Roles.Events;

namespace Mangefy.Domain.Roles;

public sealed class TenantRole : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsOwnerRole { get; private set; }

    /// <summary>
    /// Indica que este cargo foi gerado a partir de um RoleTemplate no onboarding.
    /// Cargos de template não podem ser deletados pelo Owner — apenas não usados.
    /// </summary>
    public bool IsFromTemplate { get; private set; }

    /// <summary>
    /// Id do RoleTemplate de origem. Rastreabilidade — não cria dependência em runtime.
    /// </summary>
    public Guid? TemplateId { get; private set; }

    /// <summary>
    /// False quando o tenant faz downgrade e o cargo excede o novo MaxCustomRoles.
    /// O Owner deve reatribuir os funcionários vinculados.
    /// </summary>
    public bool IsActive { get; private set; }

    private readonly List<string> _permissions = [];
    public IReadOnlyCollection<string> Permissions => _permissions.AsReadOnly();

    private TenantRole() { }

    /// <summary>
    /// Cria um cargo a partir de um RoleTemplate no onboarding do tenant.
    /// Snapshot independente — alterações futuras no template não afetam este cargo.
    /// </summary>
    public static TenantRole CreateFromTemplate(
        Guid tenantId, string name, string? description, IEnumerable<string> permissions, Guid templateId)
    {
        var role = new TenantRole
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Description = description?.Trim(),
            IsOwnerRole = false,
            IsFromTemplate = true,
            TemplateId = templateId,
            IsActive = true
        };

        role._permissions.AddRange(permissions.Distinct());
        return role;
    }

    /// <summary>
    /// Cria o cargo imutável do dono do estabelecimento.
    /// Sempre criado no onboarding; possui todas as permissões sem restrição de plano.
    /// </summary>
    public static TenantRole CreateOwnerRole(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        return new TenantRole
        {
            TenantId = tenantId,
            Name = "Dono",
            Description = "Cargo imutável do proprietário do estabelecimento. Acesso irrestrito.",
            IsOwnerRole = true,
            IsFromTemplate = false,
            TemplateId = null,
            IsActive = true
        };
    }

    /// <summary>
    /// Cria um cargo customizado pelo Owner (requer plano com MaxCustomRoles > 0).
    /// </summary>
    public static TenantRole Create(Guid tenantId, string name, string? description = null)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do cargo não pode ser vazio.");

        var role = new TenantRole
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Description = description?.Trim(),
            IsOwnerRole = false,
            IsFromTemplate = false,
            TemplateId = null,
            IsActive = true
        };

        role.AddDomainEvent(new TenantRoleCreatedEvent(role.Id, tenantId, role.Name));
        return role;
    }

    public void UpdateInfo(string name, string? description)
    {
        EnsureEditable();

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do cargo não pode ser vazio.");

        Name = name.Trim();
        Description = description?.Trim();
        SetUpdatedAt();
    }

    public void GrantPermission(string permission)
    {
        EnsureEditable();

        if (!PermissionCatalog.IsValid(permission))
            throw new DomainException($"Permissão '{permission}' não existe no catálogo.");

        if (!_permissions.Contains(permission))
        {
            _permissions.Add(permission);
            SetUpdatedAt();
        }
    }

    public void RevokePermission(string permission)
    {
        EnsureEditable();
        _permissions.Remove(permission);
        SetUpdatedAt();
    }

    public void SetPermissions(IEnumerable<string> permissions)
    {
        EnsureEditable();

        var list = permissions.ToList();
        var invalid = list.Where(p => !PermissionCatalog.IsValid(p)).ToList();

        if (invalid.Any())
            throw new DomainException($"Permissões inválidas: {string.Join(", ", invalid)}");

        _permissions.Clear();
        _permissions.AddRange(list.Distinct());
        SetUpdatedAt();
    }

    /// <summary>
    /// Desativa o cargo após downgrade de plano. Funcionários vinculados perdem acesso.
    /// O Owner deve reatribuir manualmente.
    /// </summary>
    public void DeactivateByPlanDowngrade()
    {
        if (IsOwnerRole)
            throw new DomainException("O cargo do dono do restaurante não pode ser desativado.");

        if (IsFromTemplate)
            throw new DomainException("Apenas cargos customizados são desativados por downgrade de plano.");

        IsActive = false;
        SetUpdatedAt();
    }

    /// <summary>
    /// Reativa o cargo após o tenant fazer upgrade de plano.
    /// </summary>
    public void ReactivateAfterUpgrade()
    {
        IsActive = true;
        SetUpdatedAt();
    }

    public bool HasPermission(string permission)
    {
        if (IsOwnerRole) return true;
        if (!IsActive) return false;
        return _permissions.Contains(permission);
    }

    private void EnsureEditable()
    {
        if (IsOwnerRole)
            throw new DomainException("O cargo do dono do restaurante não pode ser modificado.");

        if (!IsActive)
            throw new DomainException("Cargo inativo não pode ser modificado. Reative-o primeiro.");
    }
}
