using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Platform.Suppliers;
using Mangefy.Domain.Platform.Suppliers.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.PlatformSuppliers.Commands.UpdatePlatformSupplier;

public sealed class UpdatePlatformSupplierCommandHandler : IRequestHandler<UpdatePlatformSupplierCommand>
{
    private readonly IPlatformSupplierRepository _suppliers;
    private readonly IUnitOfWork _uow;

    public UpdatePlatformSupplierCommandHandler(IPlatformSupplierRepository suppliers, IUnitOfWork uow)
    {
        _suppliers = suppliers;
        _uow = uow;
    }

    public async Task Handle(UpdatePlatformSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _suppliers.GetByIdAsync(request.SupplierId, cancellationToken)
            ?? throw new NotFoundException(nameof(PlatformSupplier), request.SupplierId);

        supplier.Update(request.Name, request.SupplierCategoryId, request.Cnpj, request.Website, request.Email, request.Phone, request.Description);
        await _suppliers.UpdateAsync(supplier, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
