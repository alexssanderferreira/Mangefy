using Mangefy.Domain.Common;

namespace Mangefy.Domain.Menus;

public sealed class Menu : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>
    /// Cardápio padrão — sempre visível, independente de horário ou ativação manual.
    /// Apenas um cardápio por tenant pode ser padrão.
    /// </summary>
    public bool IsDefault { get; private set; }

    /// <summary>
    /// Vigência automática por horário. Nulo = apenas ativação manual via IsActive.
    /// </summary>
    public MenuSchedule? Schedule { get; private set; }

    private readonly List<MenuCategory> _categories = [];
    public IReadOnlyCollection<MenuCategory> Categories => _categories.AsReadOnly();

    private Menu() { }

    /// <summary>
    /// Cria o cardápio padrão do tenant. Chamado no onboarding.
    /// </summary>
    public static Menu CreateDefault(Guid tenantId, string name = "Cardápio Principal")
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        return new Menu
        {
            TenantId = tenantId,
            Name = name.Trim(),
            IsActive = true,
            IsDefault = true,
            Schedule = null
        };
    }

    /// <summary>
    /// Cria um cardápio adicional (ex: almoço, happy hour). Requer features.multi_menu.
    /// </summary>
    public static Menu Create(Guid tenantId, string name, MenuSchedule? schedule = null)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do cardápio não pode ser vazio.");

        return new Menu
        {
            TenantId = tenantId,
            Name = name.Trim(),
            IsActive = false,
            IsDefault = false,
            Schedule = schedule
        };
    }

    public void UpdateInfo(string name, MenuSchedule? schedule)
    {
        if (IsDefault && schedule is not null)
            throw new DomainException("O cardápio padrão não pode ter horário de vigência.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do cardápio não pode ser vazio.");

        Name = name.Trim();
        Schedule = schedule;
        SetUpdatedAt();
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        if (IsDefault)
            throw new DomainException("O cardápio padrão não pode ser desativado.");

        IsActive = false;
        SetUpdatedAt();
    }

    public bool IsVisibleAt(DayOfWeek day, TimeOnly time)
    {
        if (IsDefault) return true;
        if (!IsActive) return false;
        if (Schedule is null) return IsActive;
        return Schedule.IsActiveAt(day, time);
    }

    public MenuCategory AddCategory(string name, int displayOrder = 0, string? description = null)
    {
        var category = MenuCategory.Create(TenantId, name, displayOrder, description);
        _categories.Add(category);
        SetUpdatedAt();
        return category;
    }

    public void RemoveCategory(Guid categoryId)
    {
        var category = GetCategory(categoryId);

        if (category.Items.Any())
            throw new DomainException("Não é possível remover uma categoria que contém itens.");

        _categories.Remove(category);
        SetUpdatedAt();
    }

    public void UpdateCategory(Guid categoryId, string name, string? description, int displayOrder)
    {
        var category = GetCategory(categoryId);
        category.UpdateInfo(name, description, displayOrder);
        SetUpdatedAt();
    }

    public void ActivateCategory(Guid categoryId) => GetCategory(categoryId).Activate();
    public void DeactivateCategory(Guid categoryId) => GetCategory(categoryId).Deactivate();

    public MenuItem AddItemToCategory(
        Guid categoryId,
        string name,
        string? description,
        decimal price,
        string? imageUrl = null,
        bool requiresKds = true)
    {
        var category = GetCategory(categoryId);
        var item = category.AddItem(name, description, price, imageUrl, requiresKds);
        SetUpdatedAt();
        return item;
    }

    public void UpdateItem(
        Guid categoryId,
        Guid itemId,
        string name,
        string? description,
        decimal price,
        string? imageUrl,
        bool requiresKds,
        MenuItemStation station = MenuItemStation.Kitchen)
    {
        var category = GetCategory(categoryId);
        var item = GetItem(category, itemId);
        item.UpdateInfo(name, description, price, imageUrl, requiresKds, station);
        SetUpdatedAt();
    }

    public void RemoveItemFromCategory(Guid categoryId, Guid itemId)
    {
        var category = GetCategory(categoryId);
        category.RemoveItem(itemId);
        SetUpdatedAt();
    }

    public void ChangeItemStatus(Guid categoryId, Guid itemId, MenuItemStatus status)
    {
        var category = GetCategory(categoryId);
        var item = GetItem(category, itemId);

        switch (status)
        {
            case MenuItemStatus.Available: item.MarkAsAvailable(); break;
            case MenuItemStatus.Unavailable: item.MarkAsUnavailable(); break;
            case MenuItemStatus.OutOfStock: item.MarkAsOutOfStock(); break;
        }

        SetUpdatedAt();
    }

    public void SetItemPromotionalPrice(Guid categoryId, Guid itemId, decimal promoPrice, DateTime? validUntil)
    {
        var category = GetCategory(categoryId);
        GetItem(category, itemId).SetPromotionalPrice(promoPrice, validUntil);
        SetUpdatedAt();
    }

    public void ClearItemPromotionalPrice(Guid categoryId, Guid itemId)
    {
        var category = GetCategory(categoryId);
        GetItem(category, itemId).ClearPromotionalPrice();
        SetUpdatedAt();
    }

    public void SetItemRecipe(Guid categoryId, Guid itemId, IEnumerable<RecipeIngredient> ingredients)
    {
        var category = GetCategory(categoryId);
        GetItem(category, itemId).SetRecipe(ingredients);
        SetUpdatedAt();
    }

    public void ClearItemRecipe(Guid categoryId, Guid itemId)
    {
        var category = GetCategory(categoryId);
        GetItem(category, itemId).ClearRecipe();
        SetUpdatedAt();
    }

    public MenuItem? FindItem(Guid itemId) =>
        _categories.SelectMany(c => c.Items).FirstOrDefault(i => i.Id == itemId);

    private MenuCategory GetCategory(Guid categoryId) =>
        _categories.FirstOrDefault(c => c.Id == categoryId)
        ?? throw new DomainException("Categoria não encontrada neste cardápio.");

    private static MenuItem GetItem(MenuCategory category, Guid itemId) =>
        category.Items.FirstOrDefault(i => i.Id == itemId)
        ?? throw new DomainException("Item não encontrado nesta categoria.");
}
