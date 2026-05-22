using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Platform.SupplierCategories;
using Mangefy.Domain.Platform.SupplierCategories.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.SupplierCategories.Commands.CreateGlobalSupplierCategory;

public sealed class CreateGlobalSupplierCategoryCommandHandler : IRequestHandler<CreateGlobalSupplierCategoryCommand, Guid>
{
    private readonly ISupplierCategoryRepository _categories;
    private readonly IUnitOfWork _uow;

    public CreateGlobalSupplierCategoryCommandHandler(ISupplierCategoryRepository categories, IUnitOfWork uow)
    {
        _categories = categories;
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateGlobalSupplierCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = SupplierCategory.CreateGlobal(request.Name, request.Description);
        await _categories.AddAsync(category, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return category.Id;
    }
}
