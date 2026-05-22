using Mangefy.Domain.Common;
using Mangefy.Domain.Common.ValueObjects;
using Mangefy.Domain.Menus;
using Mangefy.Domain.Tabs.Events;

namespace Mangefy.Domain.Tabs;

/// <summary>
/// Comanda — representa a conta de uma pessoa durante sua permanência no estabelecimento.
/// Agrupa múltiplos pedidos (rounds) e os pagamentos ao fechar.
/// </summary>
public sealed class Tab : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public int Number { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public Guid? CurrentTableId { get; private set; }
    public string? LocationNote { get; private set; }
    public Guid OpenedByEmployeeId { get; private set; }
    public TabStatus Status { get; private set; }
    public DateTime OpenedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public uint RowVersion { get; private set; }
    public SaleChannel Channel { get; private set; }
    public DeliveryInfo? DeliveryInfo { get; private set; }

    // Ajustes financeiros aplicados ao fechar
    public Money DiscountAmount { get; private set; } = Money.Zero();
    public Money ServiceFee { get; private set; } = Money.Zero();
    public Money Tip { get; private set; } = Money.Zero();

    private readonly List<Order> _orders = [];
    private readonly List<Payment> _payments = [];

    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    public Money Subtotal => _orders
        .Where(o => o.Status != OrderStatus.Cancelled)
        .Aggregate(Money.Zero(), (acc, o) => acc.Add(o.Total));

    public Money Total
    {
        get
        {
            var subtotal = Subtotal.Amount - DiscountAmount.Amount + ServiceFee.Amount + Tip.Amount;
            return Money.Create(Math.Max(0, Math.Round(subtotal, 2)));
        }
    }

    public Money TotalPaid => _payments
        .Aggregate(Money.Zero(), (acc, p) => acc.Add(p.Amount));

    private Tab() { }

    public static Tab Open(
        Guid tenantId,
        int number,
        string customerName,
        Guid openedByEmployeeId,
        Guid? tableId = null,
        string? locationNote = null,
        SaleChannel channel = SaleChannel.InPerson,
        DeliveryInfo? deliveryInfo = null)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        if (number <= 0)
            throw new DomainException("Número da comanda deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("Nome do cliente é obrigatório na comanda.");

        if (openedByEmployeeId == Guid.Empty)
            throw new DomainException("Funcionário inválido.");

        if (channel == SaleChannel.InPerson && tableId is null && string.IsNullOrWhiteSpace(locationNote))
            throw new DomainException("Informe a mesa ou uma localização (ex: 'Balcão') para a comanda.");

        if (channel == SaleChannel.Delivery && deliveryInfo is null)
            throw new DomainException("Informações de entrega são obrigatórias para comandas de delivery.");

        var tab = new Tab
        {
            TenantId = tenantId,
            Number = number,
            CustomerName = customerName.Trim(),
            CurrentTableId = tableId,
            LocationNote = locationNote?.Trim(),
            OpenedByEmployeeId = openedByEmployeeId,
            Status = TabStatus.Open,
            OpenedAt = DateTime.UtcNow,
            Channel = channel,
            DeliveryInfo = deliveryInfo
        };

        tab.AddDomainEvent(new TabOpenedEvent(tab.Id, tenantId, tableId, tab.CustomerName));
        return tab;
    }

    // ── Localização ──────────────────────────────────────────────────────────

    public void ChangeLocation(Guid? tableId, string? locationNote)
    {
        EnsureOpen();

        if (tableId is null && string.IsNullOrWhiteSpace(locationNote))
            throw new DomainException("Informe a mesa ou uma localização para mover a comanda.");

        CurrentTableId = tableId;
        LocationNote = locationNote?.Trim();
        SetUpdatedAt();
    }

    // ── Pedidos ───────────────────────────────────────────────────────────────

    public Order AddOrder(Guid employeeId)
    {
        EnsureOpen();

        var order = Order.Create(Id, TenantId, CurrentTableId, LocationNote, employeeId);
        _orders.Add(order);
        SetUpdatedAt();
        return order;
    }

    public void AddItemToOrder(
        Guid orderId,
        Guid menuItemId,
        string itemName,
        decimal unitPrice,
        int quantity,
        bool requiresKds,
        MenuItemStation station = MenuItemStation.Kitchen,
        string? notes = null,
        IEnumerable<string>? modifiers = null)
    {
        EnsureOpen();
        GetOrder(orderId).AddItem(menuItemId, itemName, unitPrice, quantity, requiresKds, station, notes, modifiers);
        SetUpdatedAt();
    }

    public void UpdateOrderItemQuantity(Guid orderId, Guid itemId, int quantity)
    {
        EnsureOpen();
        GetOrder(orderId).UpdateItemQuantity(itemId, quantity);
        SetUpdatedAt();
    }

    public void UpdateOrderItemNotes(Guid orderId, Guid itemId, string? notes)
    {
        EnsureOpen();
        GetOrder(orderId).UpdateItemNotes(itemId, notes);
        SetUpdatedAt();
    }

    public void RemoveOrderItem(Guid orderId, Guid itemId)
    {
        EnsureOpen();
        GetOrder(orderId).RemoveItem(itemId);
        SetUpdatedAt();
    }

    public void ApplyItemDiscount(Guid orderId, Guid itemId, decimal discountAmount)
    {
        EnsureOpen();
        GetOrder(orderId).ApplyItemDiscount(itemId, discountAmount);
        SetUpdatedAt();
    }

    public void SetItemPriority(Guid orderId, Guid itemId, int priority)
    {
        EnsureOpen();
        GetOrder(orderId).SetItemPriority(itemId, priority);
        SetUpdatedAt();
    }

    public void SubmitOrder(Guid orderId)
    {
        EnsureOpen();
        var order = GetOrder(orderId);
        order.Submit();
        SetUpdatedAt();
        AddDomainEvent(new OrderSubmittedEvent(orderId, Id, TenantId, order.TableId, CustomerName));
    }

    // ── KDS — chamado pela cozinha ────────────────────────────────────────────

    public void StartItemPreparation(Guid orderId, Guid itemId)
    {
        GetOrder(orderId).StartItemPreparation(itemId);
        SetUpdatedAt();
    }

    public void MarkItemReady(Guid orderId, Guid itemId)
    {
        var order = GetOrder(orderId);
        order.MarkItemReady(itemId);
        SetUpdatedAt();

        if (order.Status == OrderStatus.Ready)
            AddDomainEvent(new OrderReadyEvent(orderId, Id, TenantId, order.TableId, CustomerName));
    }

    public void DeliverItem(Guid orderId, Guid itemId)
    {
        GetOrder(orderId).DeliverItem(itemId);
        SetUpdatedAt();
    }

    public void ReturnItem(Guid orderId, Guid itemId)
    {
        GetOrder(orderId).ReturnItem(itemId);
        SetUpdatedAt();
    }

    public void CancelItem(Guid orderId, Guid itemId, string? reason = null)
    {
        EnsureOpen();
        GetOrder(orderId).CancelItem(itemId, reason);
        SetUpdatedAt();
    }

    public void CancelOrder(Guid orderId)
    {
        EnsureOpen();
        GetOrder(orderId).Cancel();
        SetUpdatedAt();
    }

    // ── Ajustes financeiros ───────────────────────────────────────────────────

    public void ApplyTabDiscount(decimal amount)
    {
        EnsureOpen();

        if (amount > Subtotal.Amount)
            throw new DomainException("Desconto não pode exceder o subtotal da comanda.");

        DiscountAmount = Money.Create(amount); // Money.Create valida >= 0
        SetUpdatedAt();
    }

    public void ApplyServiceFee(decimal amount)
    {
        EnsureOpen();
        ServiceFee = Money.Create(amount);
        SetUpdatedAt();
    }

    public void ApplyTip(decimal amount)
    {
        EnsureOpen();
        Tip = Money.Create(amount);
        SetUpdatedAt();
    }

    // ── Fechamento e pagamento ────────────────────────────────────────────────

    public void AddPayment(decimal amount, PaymentMethod method, decimal changeGiven = 0m, string? externalReference = null)
    {
        EnsureOpen();

        if (amount <= 0)
            throw new DomainException("Valor do pagamento deve ser maior que zero.");

        var remaining = Total.Amount - TotalPaid.Amount;

        if (amount - changeGiven > remaining + 0.01m)
            throw new DomainException(
                $"Valor líquido informado (R$ {amount - changeGiven:F2}) excede o saldo restante (R$ {remaining:F2}).");

        _payments.Add(Payment.Create(Id, amount, method, changeGiven, externalReference));
        SetUpdatedAt();
    }

    public void Close()
    {
        EnsureOpen();

        var diff = Total.Amount - TotalPaid.Amount;

        if (diff > 0.01m)
            throw new DomainException(
                $"Saldo em aberto: R$ {diff:F2}. Registre o pagamento antes de fechar.");

        Status = TabStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        SetUpdatedAt();
        AddDomainEvent(new TabClosedEvent(Id, TenantId, CurrentTableId, Total.Amount));
    }

    public void Cancel(string reason)
    {
        EnsureOpen();

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Motivo do cancelamento é obrigatório.");

        foreach (var order in _orders.Where(o => o.Status != OrderStatus.Cancelled))
            order.Cancel();

        Status = TabStatus.Cancelled;
        ClosedAt = DateTime.UtcNow;
        SetUpdatedAt();
        AddDomainEvent(new TabCancelledEvent(Id, TenantId, CurrentTableId, reason));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void EnsureOpen()
    {
        if (Status != TabStatus.Open)
            throw new DomainException("Comanda não está aberta.");
    }

    private Order GetOrder(Guid orderId) =>
        _orders.FirstOrDefault(o => o.Id == orderId)
        ?? throw new DomainException("Pedido não encontrado nesta comanda.");
}
