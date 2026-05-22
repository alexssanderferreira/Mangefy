using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Platform.Suppliers.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.PlatformSuppliers.Commands.DeletePlatformSupplier;

public sealed class DeletePlatformSupplierCommandHandler : IRequestHandler<DeletePlatformSupplierCommand>
{
    private readonly IPlatformSupplierRepository _suppliers;
    private readonly IUnitOfWork _uow;

    public DeletePlatformSupplierCommandHandler(IPlatformSupplierRepository suppliers, IUnitOfWork uow)
    {
        _suppliers = suppliers;
        _uow = uow;
    }

    public async Task Handle(DeletePlatformSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _suppliers.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("PlatformSupplier", request.Id);

        await _suppliers.DeleteAsync(supplier, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
