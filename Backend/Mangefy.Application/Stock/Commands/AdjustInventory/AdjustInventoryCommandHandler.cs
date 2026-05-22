using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Stock.Repositories;
using MediatR;

namespace Mangefy.Application.Stock.Commands.AdjustInventory;

public sealed class AdjustInventoryCommandHandler : IRequestHandler<AdjustInventoryCommand>
{
    private readonly IStockRepository _stock;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _uow;

    public AdjustInventoryCommandHandler(IStockRepository stock, ICurrentUser currentUser, IUnitOfWork uow)
    {
        _stock = stock;
        _currentUser = currentUser;
        _uow = uow;
    }

    public async Task Handle(AdjustInventoryCommand request, CancellationToken cancellationToken)
    {
        var stock = await _stock.GetByTenantIdAsync(request.TenantId, cancellationToken)
            ?? throw new NotFoundException("Estoque", request.TenantId);

        stock.AdjustInventory(request.StockItemId, request.NewQuantity, request.Reason, _currentUser.EmployeeId ?? Guid.Empty);
        await _stock.UpdateAsync(stock, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
