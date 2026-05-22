using Mangefy.Domain.Common;
using Mangefy.Domain.Common.ValueObjects;

namespace Mangefy.Domain.Stock;

public sealed class StockItem : Entity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public StockUnit Unit { get; private set; }
    public decimal CurrentQuantity { get; private set; }
    public decimal MinimumQuantity { get; private set; }
    public Money CostPerUnit { get; private set; }
    public Guid? SupplierId { get; private set; }

    /// <summary>
    /// Setor onde o item é utilizado — usado para filtro. Estoque é global por tenant.
    /// </summary>
    public StockStation Station { get; private set; }

    private StockItem() { }

    internal static StockItem Create(
        Guid tenantId,
        string name,
        StockUnit unit,
        decimal currentQuantity,
        decimal minimumQuantity,
        decimal costPerUnit,
        StockStation station,
        Guid? supplierId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do item de estoque não pode ser vazio.");

        if (currentQuantity < 0)
            throw new DomainException("Quantidade atual não pode ser negativa.");

        if (minimumQuantity < 0)
            throw new DomainException("Estoque mínimo não pode ser negativo.");

        return new StockItem
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Unit = unit,
            CurrentQuantity = currentQuantity,
            MinimumQuantity = minimumQuantity,
            CostPerUnit = Money.Create(costPerUnit),
            Station = station,
            SupplierId = supplierId
        };
    }

    internal void UpdateInfo(
        string name,
        StockUnit unit,
        decimal minimumQuantity,
        decimal costPerUnit,
        StockStation station,
        Guid? supplierId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do item de estoque não pode ser vazio.");

        if (minimumQuantity < 0)
            throw new DomainException("Estoque mínimo não pode ser negativo.");

        Name = name.Trim();
        Unit = unit;
        MinimumQuantity = minimumQuantity;
        CostPerUnit = Money.Create(costPerUnit);
        Station = station;
        SupplierId = supplierId;
        SetUpdatedAt();
    }

    internal void AddQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantidade a adicionar deve ser maior que zero.");

        CurrentQuantity += quantity;
        SetUpdatedAt();
    }

    internal void DeductQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantidade a deduzir deve ser maior que zero.");

        if (CurrentQuantity - quantity < 0)
            throw new DomainException($"Estoque insuficiente para '{Name}'. Disponível: {CurrentQuantity} {Unit}.");

        CurrentQuantity -= quantity;
        SetUpdatedAt();
    }

    internal void AdjustQuantity(decimal newQuantity)
    {
        if (newQuantity < 0)
            throw new DomainException("Quantidade ajustada não pode ser negativa.");

        CurrentQuantity = newQuantity;
        SetUpdatedAt();
    }

    public bool IsBelowMinimum() => CurrentQuantity <= MinimumQuantity;
}
