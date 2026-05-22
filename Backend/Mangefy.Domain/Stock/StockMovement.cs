using Mangefy.Domain.Common;

namespace Mangefy.Domain.Stock;

public sealed class StockMovement : Entity
{
    public Guid TenantId { get; private set; }
    public Guid StockItemId { get; private set; }
    public StockMovementType Type { get; private set; }

    /// <summary>
    /// Valor positivo = entrada. Valor negativo = saída.
    /// </summary>
    public decimal Quantity { get; private set; }

    public string? Reason { get; private set; }
    public Guid? EmployeeId { get; private set; }

    /// <summary>
    /// Preenchido apenas em movimentações do tipo Sale (baixa automática).
    /// </summary>
    public Guid? OrderItemId { get; private set; }

    private StockMovement() { }

    internal static StockMovement CreatePurchase(
        Guid tenantId, Guid stockItemId, decimal quantity, string? reason, Guid employeeId)
    {
        return new StockMovement
        {
            TenantId = tenantId,
            StockItemId = stockItemId,
            Type = StockMovementType.Purchase,
            Quantity = quantity,
            Reason = reason?.Trim(),
            EmployeeId = employeeId
        };
    }

    internal static StockMovement CreateSale(
        Guid tenantId, Guid stockItemId, decimal quantity, Guid orderItemId)
    {
        return new StockMovement
        {
            TenantId = tenantId,
            StockItemId = stockItemId,
            Type = StockMovementType.Sale,
            Quantity = -quantity,
            OrderItemId = orderItemId
        };
    }

    internal static StockMovement CreateManualOut(
        Guid tenantId, Guid stockItemId, decimal quantity, StockMovementType type, string reason, Guid employeeId)
    {
        if (type is StockMovementType.Purchase or StockMovementType.Sale)
            throw new DomainException("Use os métodos específicos para Purchase e Sale.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Motivo é obrigatório para movimentações manuais de saída.");

        return new StockMovement
        {
            TenantId = tenantId,
            StockItemId = stockItemId,
            Type = type,
            Quantity = -quantity,
            Reason = reason.Trim(),
            EmployeeId = employeeId
        };
    }

    internal static StockMovement CreateInventoryAdjustment(
        Guid tenantId, Guid stockItemId, decimal difference, string reason, Guid employeeId)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Motivo é obrigatório para ajuste de inventário.");

        return new StockMovement
        {
            TenantId = tenantId,
            StockItemId = stockItemId,
            Type = StockMovementType.InventoryAdjustment,
            Quantity = difference,
            Reason = reason.Trim(),
            EmployeeId = employeeId
        };
    }
}
