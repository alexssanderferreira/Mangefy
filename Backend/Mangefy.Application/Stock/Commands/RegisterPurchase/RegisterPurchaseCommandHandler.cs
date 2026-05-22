using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Stock.Repositories;
using MediatR;

namespace Mangefy.Application.Stock.Commands.RegisterPurchase;

public sealed class RegisterPurchaseCommandHandler : IRequestHandler<RegisterPurchaseCommand>
{
    private readonly IStockRepository _stocks;
    private readonly IUnitOfWork _uow;

    public RegisterPurchaseCommandHandler(IStockRepository stocks, IUnitOfWork uow)
    {
        _stocks = stocks;
        _uow = uow;
    }

    public async Task Handle(RegisterPurchaseCommand request, CancellationToken cancellationToken)
    {
        var stock = await _stocks.GetByTenantIdAsync(request.TenantId, cancellationToken)
            ?? throw new NotFoundException("Stock", request.TenantId);

        stock.RegisterPurchase(request.StockItemId, request.Quantity, request.Reason, request.EmployeeId);
        await _stocks.UpdateAsync(stock, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
