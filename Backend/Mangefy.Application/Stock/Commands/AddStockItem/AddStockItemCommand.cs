using Mangefy.Domain.Stock;
using MediatR;

namespace Mangefy.Application.Stock.Commands.AddStockItem;

public sealed record AddStockItemCommand(
    Guid TenantId,
    string Name,
    StockUnit Unit,
    decimal CurrentQuantity,
    decimal MinimumQuantity,
    decimal CostPerUnit,
    StockStation Station,
    Guid? SupplierId
) : IRequest<Guid>;
