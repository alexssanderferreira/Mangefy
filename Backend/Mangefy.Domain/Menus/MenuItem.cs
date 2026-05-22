using Mangefy.Domain.Common;
using Mangefy.Domain.Common.ValueObjects;

namespace Mangefy.Domain.Menus;

public sealed class MenuItem : Entity
{
    public Guid TenantId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Money Price { get; private set; } = null!;
    public string? ImageUrl { get; private set; }
    public bool RequiresKds { get; private set; }
    public MenuItemStation Station { get; private set; }
    public MenuItemStatus Status { get; private set; }

    /// <summary>Preço promocional. Nulo = sem promoção ativa.</summary>
    public Money? PromotionalPrice { get; private set; }

    /// <summary>Validade da promoção. Nulo = sem validade (até desativar manualmente).</summary>
    public DateTime? PromotionValidUntil { get; private set; }

    private readonly List<RecipeIngredient> _recipe = [];
    private readonly List<PriceHistoryEntry> _priceHistory = [];

    public IReadOnlyList<RecipeIngredient> Recipe => _recipe.AsReadOnly();
    public IReadOnlyList<PriceHistoryEntry> PriceHistory => _priceHistory.AsReadOnly();

    private MenuItem() { }

    internal static MenuItem Create(
        Guid tenantId,
        Guid categoryId,
        string name,
        string? description,
        decimal price,
        string? imageUrl,
        bool requiresKds,
        MenuItemStation station = MenuItemStation.Kitchen)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do item não pode ser vazio.");

        return new MenuItem
        {
            TenantId = tenantId,
            CategoryId = categoryId,
            Name = name.Trim(),
            Description = description?.Trim(),
            Price = Money.Create(price),
            ImageUrl = imageUrl?.Trim(),
            RequiresKds = requiresKds,
            Station = station,
            Status = MenuItemStatus.Available
        };
    }

    internal void UpdateInfo(string name, string? description, decimal price, string? imageUrl, bool requiresKds, MenuItemStation station, Guid? changedByEmployeeId = null, string? priceChangeReason = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do item não pode ser vazio.");

        if (price != Price.Amount)
            _priceHistory.Add(PriceHistoryEntry.Create(Price.Amount, price, changedByEmployeeId, priceChangeReason));

        Name = name.Trim();
        Description = description?.Trim();
        Price = Money.Create(price);
        ImageUrl = imageUrl?.Trim();
        RequiresKds = requiresKds;
        Station = station;
        SetUpdatedAt();
    }

    /// <summary>
    /// Define preço promocional. Pedidos submetidos durante a promoção válida
    /// devem usar GetEffectivePrice() para capturar o snapshot correto.
    /// </summary>
    internal void SetPromotionalPrice(decimal promoPrice, DateTime? validUntil)
    {
        if (promoPrice <= 0)
            throw new DomainException("Preço promocional deve ser maior que zero.");

        if (promoPrice >= Price.Amount)
            throw new DomainException("Preço promocional deve ser menor que o preço regular.");

        if (validUntil.HasValue && validUntil.Value <= DateTime.UtcNow)
            throw new DomainException("Validade da promoção deve ser no futuro.");

        PromotionalPrice = Money.Create(promoPrice);
        PromotionValidUntil = validUntil;
        SetUpdatedAt();
    }

    internal void ClearPromotionalPrice()
    {
        PromotionalPrice = null;
        PromotionValidUntil = null;
        SetUpdatedAt();
    }

    /// <summary>
    /// Retorna o preço efetivo atual (promocional se ativo, regular caso contrário).
    /// Use este método ao criar snapshots de preço em OrderItem.
    /// </summary>
    public decimal GetEffectivePrice()
    {
        if (PromotionalPrice is null) return Price.Amount;

        if (PromotionValidUntil.HasValue && DateTime.UtcNow > PromotionValidUntil.Value)
            return Price.Amount;

        return PromotionalPrice.Amount;
    }

    internal void SetRecipe(IEnumerable<RecipeIngredient> ingredients)
    {
        _recipe.Clear();
        _recipe.AddRange(ingredients);
        SetUpdatedAt();
    }

    internal void ClearRecipe()
    {
        _recipe.Clear();
        SetUpdatedAt();
    }

    public bool HasRecipe() => _recipe.Count > 0;

    internal void MarkAsUnavailable()
    {
        Status = MenuItemStatus.Unavailable;
        SetUpdatedAt();
    }

    internal void MarkAsOutOfStock()
    {
        Status = MenuItemStatus.OutOfStock;
        SetUpdatedAt();
    }

    internal void MarkAsAvailable()
    {
        Status = MenuItemStatus.Available;
        SetUpdatedAt();
    }

    public bool IsAvailable() => Status == MenuItemStatus.Available;
}
