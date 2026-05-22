using Mangefy.Domain.Common;

namespace Mangefy.Domain.Menus;

public sealed class MenuCategory : Entity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<MenuItem> _items = [];
    public IReadOnlyCollection<MenuItem> Items => _items.AsReadOnly();

    private MenuCategory() { }

    internal static MenuCategory Create(Guid tenantId, string name, int displayOrder = 0, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da categoria não pode ser vazio.");

        return new MenuCategory
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Description = description?.Trim(),
            DisplayOrder = displayOrder,
            IsActive = true
        };
    }

    internal void UpdateInfo(string name, string? description, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da categoria não pode ser vazio.");

        Name = name.Trim();
        Description = description?.Trim();
        DisplayOrder = displayOrder;
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

    internal MenuItem AddItem(
        string name,
        string? description,
        decimal price,
        string? imageUrl,
        bool requiresKds)
    {
        var item = MenuItem.Create(TenantId, Id, name, description, price, imageUrl, requiresKds);
        _items.Add(item);
        SetUpdatedAt();
        return item;
    }

    internal void RemoveItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException("Item não encontrado nesta categoria.");
        _items.Remove(item);
        SetUpdatedAt();
    }
}
