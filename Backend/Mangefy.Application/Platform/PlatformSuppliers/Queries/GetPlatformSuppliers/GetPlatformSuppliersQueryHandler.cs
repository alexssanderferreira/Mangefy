using Mangefy.Domain.Platform.Suppliers.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.PlatformSuppliers.Queries.GetPlatformSuppliers;

public sealed class GetPlatformSuppliersQueryHandler : IRequestHandler<GetPlatformSuppliersQuery, IReadOnlyList<PlatformSupplierDto>>
{
    private readonly IPlatformSupplierRepository _suppliers;

    public GetPlatformSuppliersQueryHandler(IPlatformSupplierRepository suppliers) => _suppliers = suppliers;

    public async Task<IReadOnlyList<PlatformSupplierDto>> Handle(GetPlatformSuppliersQuery request, CancellationToken cancellationToken)
    {
        var list = request.CategoryId.HasValue
            ? await _suppliers.GetByCategoryAsync(request.CategoryId.Value, cancellationToken)
            : await _suppliers.GetAllAsync(cancellationToken);

        return list.Select(s => new PlatformSupplierDto(
            s.Id, s.Name, s.Cnpj, s.SupplierCategoryId,
            s.Website, s.Email?.Value, s.Phone?.Value,
            s.Description, s.IsActive)).ToList();
    }
}
