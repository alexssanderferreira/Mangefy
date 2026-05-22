using MediatR;

namespace Mangefy.Application.Platform.SupplierCategories.Queries.GetGlobalSupplierCategories;

public sealed record GetGlobalSupplierCategoriesQuery : IRequest<IReadOnlyList<SupplierCategoryDto>>;

public sealed record SupplierCategoryDto(Guid Id, string Name, string? Description, bool IsActive);
