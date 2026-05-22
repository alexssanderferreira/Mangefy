using Mangefy.Domain.Common;
using Mangefy.Domain.Stock.Events;

namespace Mangefy.Domain.Stock;

public sealed class Stock : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public uint RowVersion { get; private set; }

    private readonly List<StockItem> _items = [];
    private readonly List<StockMovement> _movements = [];

    public IReadOnlyList<StockItem> Items => _items.AsReadOnly();
    public IReadOnlyList<StockMovement> Movements => _movements.AsReadOnly();

    private Stock() { }

    public static Stock Create(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        return new Stock { TenantId = tenantId };
    }

    public StockItem AddItem(
        string name,
        StockUnit unit,
        decimal currentQuantity,
        decimal minimumQuantity,
        decimal costPerUnit,
        StockStation station,
        Guid? supplierId = null)
    {
        var item = StockItem.Create(TenantId, name, unit, currentQuantity, minimumQuantity, costPerUnit, station, supplierId);
        _items.Add(item);
        SetUpdatedAt();
        return item;
    }

    public void UpdateItem(
        Guid stockItemId,
        string name,
        StockUnit unit,
        decimal minimumQuantity,
        decimal costPerUnit,
        StockStation station,
        Guid? supplierId)
    {
        var item = GetItemOrThrow(stockItemId);
        item.UpdateInfo(name, unit, minimumQuantity, costPerUnit, station, supplierId);
        SetUpdatedAt();
    }

    public void RegisterPurchase(Guid stockItemId, decimal quantity, string? reason, Guid employeeId)
    {
        var item = GetItemOrThrow(stockItemId);
        item.AddQuantity(quantity);

        _movements.Add(StockMovement.CreatePurchase(TenantId, stockItemId, quantity, reason, employeeId));
        SetUpdatedAt();
    }

    /// <summary>
    /// Baixa automática ao vender um item do cardápio.
    /// Chamado pela Application layer ao processar OrderReadyEvent com a ficha técnica do MenuItem.
    /// </summary>
    public void DeductForSale(Guid stockItemId, decimal quantity, Guid orderItemId)
    {
        var item = GetItemOrThrow(stockItemId);
        item.DeductQuantity(quantity);

        _movements.Add(StockMovement.CreateSale(TenantId, stockItemId, quantity, orderItemId));

        if (item.IsBelowMinimum())
            AddDomainEvent(new StockLowEvent(TenantId, item.Id, item.Name, item.CurrentQuantity, item.MinimumQuantity));

        SetUpdatedAt();
    }

    public void RegisterManualOut(
        Guid stockItemId, decimal quantity, StockMovementType type, string reason, Guid employeeId)
    {
        var item = GetItemOrThrow(stockItemId);
        item.DeductQuantity(quantity);

        _movements.Add(StockMovement.CreateManualOut(TenantId, stockItemId, quantity, type, reason, employeeId));

        if (item.IsBelowMinimum())
            AddDomainEvent(new StockLowEvent(TenantId, item.Id, item.Name, item.CurrentQuantity, item.MinimumQuantity));

        SetUpdatedAt();
    }

    public void AdjustInventory(Guid stockItemId, decimal newQuantity, string reason, Guid employeeId)
    {
        var item = GetItemOrThrow(stockItemId);
        var difference = newQuantity - item.CurrentQuantity;

        item.AdjustQuantity(newQuantity);
        _movements.Add(StockMovement.CreateInventoryAdjustment(TenantId, stockItemId, difference, reason, employeeId));

        if (item.IsBelowMinimum())
            AddDomainEvent(new StockLowEvent(TenantId, item.Id, item.Name, item.CurrentQuantity, item.MinimumQuantity));

        SetUpdatedAt();
    }

    public IReadOnlyList<StockItem> GetItemsBelowMinimum() =>
        _items.Where(i => i.IsBelowMinimum()).ToList().AsReadOnly();

    public IReadOnlyList<StockItem> GetItemsByStation(StockStation station) =>
        _items.Where(i => i.Station == station).ToList().AsReadOnly();

    private StockItem GetItemOrThrow(Guid stockItemId) =>
        _items.FirstOrDefault(i => i.Id == stockItemId)
        ?? throw new DomainException("Item de estoque não encontrado.");
}
