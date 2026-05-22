using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Stock;
using Mangefy.Domain.Stock.Repositories;
using MediatR;

namespace Mangefy.Application.Stock.Commands.AddStockItem;

public sealed class AddStockItemCommandHandler : IRequestHandler<AddStockItemCommand, Guid>
{
    private readonly IStockRepository _stocks;
    private readonly IUnitOfWork _uow;

    public AddStockItemCommandHandler(IStockRepository stocks, IUnitOfWork uow)
    {
        _stocks = stocks;
        _uow = uow;
    }

    public async Task<Guid> Handle(AddStockItemCommand request, CancellationToken cancellationToken)
    {
        var stock = await _stocks.GetByTenantIdAsync(request.TenantId, cancellationToken);

        if (stock is null)
        {
            stock = Domain.Stock.Stock.Create(request.TenantId);
            await _stocks.AddAsync(stock, cancellationToken);
        }

        var item = stock.AddItem(
            request.Name, request.Unit, request.CurrentQuantity, request.MinimumQuantity,
            request.CostPerUnit, request.Station, request.SupplierId);

        await _stocks.UpdateAsync(stock, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return item.Id;
    }
}
