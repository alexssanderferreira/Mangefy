using Mangefy.Domain.Common;
using Mangefy.Domain.Common.ValueObjects;
using Mangefy.Domain.Menus;

namespace Mangefy.Domain.Tabs;

public sealed class OrderItem : Entity
{
    public Guid MenuItemId { get; private set; }
    public string ItemName { get; private set; } = string.Empty;  // snapshot do nome no momento do pedido
    public Money UnitPrice { get; private set; } = null!;         // snapshot do preço no momento do pedido
    public int Quantity { get; private set; }
    public string? Notes { get; private set; }
    public bool RequiresKds { get; private set; }
    public MenuItemStation Station { get; private set; }
    public int Priority { get; private set; }                     // 0 = normal, 1 = alta
    public OrderItemStatus Status { get; private set; }
    public DateTime? SentToKitchenAt { get; private set; }
    public DateTime? PreparationStartedAt { get; private set; }
    public DateTime? PreparedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public bool IsReturned { get; private set; }
    public decimal DiscountAmount { get; private set; }

    private readonly List<string> _modifiers = [];
    public IReadOnlyList<string> Modifiers => _modifiers.AsReadOnly();

    public Money TotalPrice => UnitPrice.Multiply(Quantity).Subtract(DiscountAmount);

    private OrderItem() { }

    internal static OrderItem Create(
        Guid menuItemId,
        string itemName,
        decimal unitPrice,
        int quantity,
        bool requiresKds,
        MenuItemStation station = MenuItemStation.Kitchen,
        string? notes = null,
        IEnumerable<string>? modifiers = null)
    {
        if (menuItemId == Guid.Empty)
            throw new DomainException("Item do cardápio inválido.");

        if (string.IsNullOrWhiteSpace(itemName))
            throw new DomainException("Nome do item não pode ser vazio.");

        if (quantity <= 0)
            throw new DomainException("Quantidade deve ser maior que zero.");

        var item = new OrderItem
        {
            MenuItemId = menuItemId,
            ItemName = itemName.Trim(),
            UnitPrice = Money.Create(unitPrice),
            Quantity = quantity,
            Notes = notes?.Trim(),
            RequiresKds = requiresKds,
            Station = station,
            Status = OrderItemStatus.Pending
        };

        if (modifiers != null)
            item._modifiers.AddRange(modifiers.Where(m => !string.IsNullOrWhiteSpace(m)).Select(m => m.Trim()));

        return item;
    }

    internal void UpdateQuantity(int quantity)
    {
        if (Status != OrderItemStatus.Pending)
            throw new DomainException($"'{ItemName}' já foi enviado — quantidade não pode ser alterada.");

        if (quantity <= 0)
            throw new DomainException("Quantidade deve ser maior que zero.");

        Quantity = quantity;
        SetUpdatedAt();
    }

    internal void UpdateNotes(string? notes)
    {
        if (Status != OrderItemStatus.Pending)
            throw new DomainException($"'{ItemName}' já foi enviado — observação não pode ser alterada.");

        Notes = notes?.Trim();
        SetUpdatedAt();
    }

    internal void ApplyDiscount(decimal amount)
    {
        if (amount < 0)
            throw new DomainException("Desconto não pode ser negativo.");

        if (amount > UnitPrice.Multiply(Quantity).Amount)
            throw new DomainException("Desconto não pode ser maior que o valor do item.");

        DiscountAmount = amount;
        SetUpdatedAt();
    }

    internal void SetPriority(int priority)
    {
        if (priority < 0)
            throw new DomainException("Prioridade inválida.");

        Priority = priority;
        SetUpdatedAt();
    }

    internal void SendToKitchen()
    {
        if (Status != OrderItemStatus.Pending)
            throw new DomainException($"'{ItemName}' já foi enviado para a cozinha.");

        Status = OrderItemStatus.Sent;
        SentToKitchenAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    internal void StartPreparing()
    {
        if (Status != OrderItemStatus.Sent && Status != OrderItemStatus.Returned)
            throw new DomainException($"'{ItemName}' não está na fila da cozinha.");

        Status = OrderItemStatus.Preparing;
        PreparationStartedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    internal void MarkAsReady()
    {
        if (Status != OrderItemStatus.Preparing)
            throw new DomainException($"'{ItemName}' não está em preparo.");

        Status = OrderItemStatus.Ready;
        PreparedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    internal void MarkAsDelivered()
    {
        if (Status != OrderItemStatus.Ready)
            throw new DomainException($"'{ItemName}' ainda não está pronto.");

        Status = OrderItemStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        IsReturned = false;
        SetUpdatedAt();
    }

    internal void Return()
    {
        if (Status != OrderItemStatus.Delivered)
            throw new DomainException($"'{ItemName}' não foi entregue — não pode ser devolvido.");

        Status = OrderItemStatus.Returned;
        IsReturned = true;
        SetUpdatedAt();
    }

    internal void Cancel(string? reason = null)
    {
        if (Status == OrderItemStatus.Cancelled)
            throw new DomainException($"'{ItemName}' já está cancelado.");

        if (Status != OrderItemStatus.Pending && string.IsNullOrWhiteSpace(reason))
            throw new DomainException($"Motivo obrigatório para cancelar '{ItemName}' após envio.");

        CancellationReason = reason?.Trim();
        Status = OrderItemStatus.Cancelled;
        SetUpdatedAt();
    }
}
