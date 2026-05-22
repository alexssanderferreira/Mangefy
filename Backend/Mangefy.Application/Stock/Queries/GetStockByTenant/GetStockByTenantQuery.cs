using MediatR;

namespace Mangefy.Application.Stock.Queries.GetStockByTenant;

public sealed record GetStockByTenantQuery(Guid TenantId)
    : IRequest<IReadOnlyList<StockItemDto>>;
