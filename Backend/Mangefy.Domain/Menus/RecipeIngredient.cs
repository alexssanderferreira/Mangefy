using Mangefy.Domain.Common;

namespace Mangefy.Domain.Menus;

/// <summary>
/// Ingrediente da ficha técnica de um MenuItem.
/// Referencia um StockItem por Id — cross-aggregate por Id (padrão DDD).
/// </summary>
public sealed class RecipeIngredient : ValueObject
{
    public Guid StockItemId { get; }
    public decimal Quantity { get; }

    private RecipeIngredient(Guid stockItemId, decimal quantity)
    {
        StockItemId = stockItemId;
        Quantity = quantity;
    }

    public static RecipeIngredient Create(Guid stockItemId, decimal quantity)
    {
        if (stockItemId == Guid.Empty)
            throw new DomainException("StockItemId inválido.");

        if (quantity <= 0)
            throw new DomainException("Quantidade do ingrediente deve ser maior que zero.");

        return new RecipeIngredient(stockItemId, quantity);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return StockItemId;
        yield return Quantity;
    }
}
