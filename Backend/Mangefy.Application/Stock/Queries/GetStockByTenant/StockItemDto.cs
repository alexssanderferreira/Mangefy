using Mangefy.Domain.Stock;

namespace Mangefy.Application.Stock.Queries.GetStockByTenant;

public sealed record StockItemDto(
    Guid Id,
    string Name,
    string Unit,
    decimal CurrentQuantity,
    decimal MinimumQuantity,
    decimal CostPerUnit,
    string Station,
    Guid? SupplierId,
    bool IsBelowMinimum
)
{
    public static StockItemDto FromDomain(StockItem i) => new(
        i.Id,
        i.Name,
        i.Unit.ToString(),
        i.CurrentQuantity,
        i.MinimumQuantity,
        i.CostPerUnit.Amount,
        i.Station.ToString(),
        i.SupplierId,
        i.IsBelowMinimum());
}
