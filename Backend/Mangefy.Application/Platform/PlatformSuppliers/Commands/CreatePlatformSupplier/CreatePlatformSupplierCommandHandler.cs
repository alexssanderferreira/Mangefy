using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Platform.Suppliers;
using Mangefy.Domain.Platform.Suppliers.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.PlatformSuppliers.Commands.CreatePlatformSupplier;

public sealed class CreatePlatformSupplierCommandHandler : IRequestHandler<CreatePlatformSupplierCommand, Guid>
{
    private readonly IPlatformSupplierRepository _suppliers;
    private readonly IUnitOfWork _uow;

    public CreatePlatformSupplierCommandHandler(IPlatformSupplierRepository suppliers, IUnitOfWork uow)
    {
        _suppliers = suppliers;
        _uow = uow;
    }

    public async Task<Guid> Handle(CreatePlatformSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = PlatformSupplier.Create(
            request.Name, request.SupplierCategoryId,
            request.Cnpj, request.Website, request.Email, request.Phone, request.Description);

        await _suppliers.AddAsync(supplier, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return supplier.Id;
    }
}
