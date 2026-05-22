namespace Mangefy.Domain.Stock;

public enum StockMovementType
{
    /// <summary>Entrada por compra de mercadoria.</summary>
    Purchase,

    /// <summary>Saída automática pela venda de um item do cardápio (via ficha técnica).</summary>
    Sale,

    /// <summary>Saída manual por consumo interno.</summary>
    ManualConsumption,

    /// <summary>Saída por perda ou vencimento de produto.</summary>
    Loss,

    /// <summary>Ajuste de quantidade após inventário físico (pode ser positivo ou negativo).</summary>
    InventoryAdjustment
}
