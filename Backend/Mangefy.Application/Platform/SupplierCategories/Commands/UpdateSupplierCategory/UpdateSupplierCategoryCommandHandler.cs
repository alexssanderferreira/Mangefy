using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Platform.SupplierCategories;
using Mangefy.Domain.Platform.SupplierCategories.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.SupplierCategories.Commands.UpdateSupplierCategory;

public sealed class UpdateSupplierCategoryCommandHandler : IRequestHandler<UpdateSupplierCategoryCommand>
{
    private readonly ISupplierCategoryRepository _categories;
    private readonly IUnitOfWork _uow;

    public UpdateSupplierCategoryCommandHandler(ISupplierCategoryRepository categories, IUnitOfWork uow)
    {
        _categories = categories;
        _uow = uow;
    }

    public async Task Handle(UpdateSupplierCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categories.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(SupplierCategory), request.CategoryId);

        category.Update(request.Name, request.Description);
        await _categories.UpdateAsync(category, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
