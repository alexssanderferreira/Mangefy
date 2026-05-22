using Mangefy.Domain.Common;
using Mangefy.Domain.Platform.BusinessTypes.Events;

namespace Mangefy.Domain.Platform.BusinessTypes;

/// <summary>
/// Tipo de negócio gerenciado pelo AdminSaas (ex: Restaurante, Bar, Padaria).
/// Define os templates de cargo que serão copiados para novos tenants deste tipo.
/// </summary>
public sealed class BusinessType : AggregateRoot
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<RoleTemplate> _roleTemplates = [];
    public IReadOnlyCollection<RoleTemplate> RoleTemplates => _roleTemplates.AsReadOnly();

    private BusinessType() { }

    public static BusinessType Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do tipo de negócio não pode ser vazio.");

        var businessType = new BusinessType
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            IsActive = true
        };

        businessType.AddDomainEvent(new BusinessTypeCreatedEvent(businessType.Id, businessType.Name));
        return businessType;
    }

    public void UpdateInfo(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do tipo de negócio não pode ser vazio.");

        Name = name.Trim();
        Description = description?.Trim();
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }

    // ── Templates de Cargo ────────────────────────────────────────────────────

    public RoleTemplate AddRoleTemplate(string name, string? description = null)
    {
        if (_roleTemplates.Any(t => t.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"Já existe um template de cargo com o nome '{name}'.");

        var template = RoleTemplate.Create(Id, name, description);
        _roleTemplates.Add(template);
        SetUpdatedAt();
        return template;
    }

    public void UpdateRoleTemplate(Guid templateId, string name, string? description)
    {
        var template = GetTemplate(templateId);
        template.UpdateInfo(name, description);
        SetUpdatedAt();
    }

    public void SetRoleTemplatePermissions(Guid templateId, IEnumerable<string> permissions)
    {
        var template = GetTemplate(templateId);
        template.SetPermissions(permissions);
        SetUpdatedAt();
    }

    public void DeactivateRoleTemplate(Guid templateId)
    {
        GetTemplate(templateId).Deactivate();
        SetUpdatedAt();
    }

    public void ActivateRoleTemplate(Guid templateId)
    {
        GetTemplate(templateId).Activate();
        SetUpdatedAt();
    }

    public void RemoveRoleTemplate(Guid templateId)
    {
        var template = GetTemplate(templateId);
        _roleTemplates.Remove(template);
        SetUpdatedAt();
    }

    /// <summary>
    /// Retorna apenas os templates ativos — usados no onboarding de novos tenants.
    /// </summary>
    public IReadOnlyList<RoleTemplate> GetActiveTemplates() =>
        _roleTemplates.Where(t => t.IsActive).ToList();

    private RoleTemplate GetTemplate(Guid templateId) =>
        _roleTemplates.FirstOrDefault(t => t.Id == templateId)
        ?? throw new DomainException("Template de cargo não encontrado.");
}
