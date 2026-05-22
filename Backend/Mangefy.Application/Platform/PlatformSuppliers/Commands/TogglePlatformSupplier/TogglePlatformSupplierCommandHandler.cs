using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Platform.Suppliers.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.PlatformSuppliers.Commands.TogglePlatformSupplier;

public sealed class TogglePlatformSupplierCommandHandler : IRequestHandler<TogglePlatformSupplierCommand>
{
    private readonly IPlatformSupplierRepository _suppliers;
    private readonly IUnitOfWork _uow;

    public TogglePlatformSupplierCommandHandler(IPlatformSupplierRepository suppliers, IUnitOfWork uow)
    {
        _suppliers = suppliers;
        _uow = uow;
    }

    public async Task Handle(TogglePlatformSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _suppliers.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("PlatformSupplier", request.Id);

        if (request.Activate) supplier.Activate();
        else supplier.Deactivate();

        await _suppliers.UpdateAsync(supplier, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
