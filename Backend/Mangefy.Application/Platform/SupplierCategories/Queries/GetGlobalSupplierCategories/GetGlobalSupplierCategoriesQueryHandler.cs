using Mangefy.Domain.Platform.SupplierCategories.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.SupplierCategories.Queries.GetGlobalSupplierCategories;

public sealed class GetGlobalSupplierCategoriesQueryHandler : IRequestHandler<GetGlobalSupplierCategoriesQuery, IReadOnlyList<SupplierCategoryDto>>
{
    private readonly ISupplierCategoryRepository _categories;

    public GetGlobalSupplierCategoriesQueryHandler(ISupplierCategoryRepository categories) => _categories = categories;

    public async Task<IReadOnlyList<SupplierCategoryDto>> Handle(GetGlobalSupplierCategoriesQuery request, CancellationToken cancellationToken)
    {
        var list = await _categories.GetAllGlobalAsync(cancellationToken);
        return list.Select(c => new SupplierCategoryDto(c.Id, c.Name, c.Description, c.IsActive)).ToList();
    }
}
