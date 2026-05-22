using Mangefy.Application.Common.Exceptions;
using Mangefy.Domain.Stock.Repositories;
using MediatR;

namespace Mangefy.Application.Stock.Queries.GetStockByTenant;

public sealed class GetStockByTenantQueryHandler
    : IRequestHandler<GetStockByTenantQuery, IReadOnlyList<StockItemDto>>
{
    private readonly IStockRepository _stock;

    public GetStockByTenantQueryHandler(IStockRepository stock)
        => _stock = stock;

    public async Task<IReadOnlyList<StockItemDto>> Handle(
        GetStockByTenantQuery request, CancellationToken cancellationToken)
    {
        var stock = await _stock.GetByTenantIdAsync(request.TenantId, cancellationToken)
            ?? throw new NotFoundException("Estoque", request.TenantId);

        return stock.Items.Select(StockItemDto.FromDomain).ToList();
    }
}
