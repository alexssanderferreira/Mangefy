using MediatR;

namespace Mangefy.Application.Platform.PlatformSuppliers.Queries.GetPlatformSuppliers;

public sealed record GetPlatformSuppliersQuery(Guid? CategoryId = null) : IRequest<IReadOnlyList<PlatformSupplierDto>>;

public sealed record PlatformSupplierDto(
    Guid Id,
    string Name,
    string? Cnpj,
    Guid SupplierCategoryId,
    string? Website,
    string? Email,
    string? Phone,
    string? Description,
    bool IsActive);
