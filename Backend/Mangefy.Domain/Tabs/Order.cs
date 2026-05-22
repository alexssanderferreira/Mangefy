using Mangefy.Domain.Common;
using Mangefy.Domain.Common.ValueObjects;
using Mangefy.Domain.Menus;
using Mangefy.Domain.Tabs.Events;

namespace Mangefy.Domain.Tabs;

/// <summary>
/// Representa um "round" de pedido — um conjunto de itens enviado à cozinha de uma vez.
/// Pertence sempre a uma Comanda (Tab).
/// </summary>
public sealed class Order : Entity
{
    public Guid TabId { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid? TableId { get; private set; }
    public string? LocationNote { get; private set; }
    public Guid EmployeeId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime? SubmittedAt { get; private set; }

    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Money Total => _items
        .Where(i => i.Status != OrderItemStatus.Cancelled)
        .Aggregate(Money.Zero(), (acc, i) => acc.Add(i.TotalPrice));

    private Order() { }

    internal static Order Create(
        Guid tabId, Guid tenantId, Guid? tableId, string? locationNote, Guid employeeId)
    {
        if (tabId == Guid.Empty)
            throw new DomainException("TabId inválido.");

        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        if (employeeId == Guid.Empty)
            throw new DomainException("Funcionário inválido.");

        if (tableId is null && string.IsNullOrWhiteSpace(locationNote))
            throw new DomainException("Informe a mesa ou uma localização para o pedido.");

        return new Order
        {
            TabId = tabId,
            TenantId = tenantId,
            TableId = tableId,
            LocationNote = locationNote?.Trim(),
            EmployeeId = employeeId,
            Status = OrderStatus.Open
        };
    }

    internal OrderItem AddItem(
        Guid menuItemId,
        string itemName,
        decimal unitPrice,
        int quantity,
        bool requiresKds,
        MenuItemStation station = MenuItemStation.Kitchen,
        string? notes = null,
        IEnumerable<string>? modifiers = null)
    {
        EnsureEditable();

        var item = OrderItem.Create(menuItemId, itemName, unitPrice, quantity, requiresKds, station, notes, modifiers);
        _items.Add(item);
        SetUpdatedAt();
        return item;
    }

    internal void UpdateItemQuantity(Guid itemId, int quantity)
    {
        EnsureEditable();
        GetItem(itemId).UpdateQuantity(quantity);
        SetUpdatedAt();
    }

    internal void UpdateItemNotes(Guid itemId, string? notes)
    {
        EnsureEditable();
        GetItem(itemId).UpdateNotes(notes);
        SetUpdatedAt();
    }

    internal void RemoveItem(Guid itemId)
    {
        EnsureEditable();
        var item = GetItem(itemId);

        if (item.Status != OrderItemStatus.Pending)
            throw new DomainException("Não é possível remover item já enviado. Cancele o item.");

        _items.Remove(item);
        SetUpdatedAt();
    }

    internal void ApplyItemDiscount(Guid itemId, decimal discountAmount)
    {
        GetItem(itemId).ApplyDiscount(discountAmount);
        SetUpdatedAt();
    }

    internal void SetItemPriority(Guid itemId, int priority)
    {
        GetItem(itemId).SetPriority(priority);
        SetUpdatedAt();
    }

    internal void Submit()
    {
        if (Status != OrderStatus.Open)
            throw new DomainException("Apenas pedidos abertos podem ser enviados.");

        var active = _items.Where(i => i.Status != OrderItemStatus.Cancelled).ToList();
        if (!active.Any())
            throw new DomainException("O pedido não possui itens.");

        foreach (var item in _items.Where(i => i.Status == OrderItemStatus.Pending && i.RequiresKds))
            item.SendToKitchen();

        Status = OrderStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    internal void StartItemPreparation(Guid itemId)
    {
        GetItem(itemId).StartPreparing();
        SyncStatus();
        SetUpdatedAt();
    }

    internal void MarkItemReady(Guid itemId)
    {
        GetItem(itemId).MarkAsReady();
        SyncStatus();
        SetUpdatedAt();
    }

    internal void DeliverItem(Guid itemId)
    {
        GetItem(itemId).MarkAsDelivered();
        SyncStatus();
        SetUpdatedAt();
    }

    internal void ReturnItem(Guid itemId)
    {
        GetItem(itemId).Return();
        SyncStatus();
        SetUpdatedAt();
    }

    internal void CancelItem(Guid itemId, string? reason = null)
    {
        if (Status is OrderStatus.Delivered or OrderStatus.Cancelled)
            throw new DomainException("Pedido já encerrado.");

        GetItem(itemId).Cancel(reason);
        SetUpdatedAt();
    }

    internal void Cancel()
    {
        if (Status is OrderStatus.Delivered or OrderStatus.Cancelled)
            throw new DomainException("Pedido já encerrado.");

        foreach (var item in _items.Where(i => i.Status != OrderItemStatus.Delivered))
            item.Cancel();

        Status = OrderStatus.Cancelled;
        SetUpdatedAt();
    }

    private void SyncStatus()
    {
        var active = _items.Where(i => i.Status != OrderItemStatus.Cancelled).ToList();
        if (!active.Any()) return;

        if (active.All(i => i.Status == OrderItemStatus.Delivered))
            Status = OrderStatus.Delivered;
        else if (active.All(i => i.Status is OrderItemStatus.Ready or OrderItemStatus.Delivered))
            Status = OrderStatus.Ready;
        else if (active.Any(i => i.Status is OrderItemStatus.Preparing or OrderItemStatus.Returned))
            Status = OrderStatus.InProgress;
    }

    private void EnsureEditable()
    {
        if (Status is OrderStatus.Cancelled or OrderStatus.Delivered)
            throw new DomainException("Pedido encerrado não pode ser modificado.");
    }

    private OrderItem GetItem(Guid itemId) =>
        _items.FirstOrDefault(i => i.Id == itemId)
        ?? throw new DomainException("Item não encontrado no pedido.");
}
