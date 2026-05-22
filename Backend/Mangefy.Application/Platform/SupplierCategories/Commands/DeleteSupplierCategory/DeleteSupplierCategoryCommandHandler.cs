using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Platform.SupplierCategories.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.SupplierCategories.Commands.DeleteSupplierCategory;

public sealed class DeleteSupplierCategoryCommandHandler : IRequestHandler<DeleteSupplierCategoryCommand>
{
    private readonly ISupplierCategoryRepository _categories;
    private readonly IUnitOfWork _uow;

    public DeleteSupplierCategoryCommandHandler(ISupplierCategoryRepository categories, IUnitOfWork uow)
    {
        _categories = categories;
        _uow = uow;
    }

    public async Task Handle(DeleteSupplierCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categories.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("SupplierCategory", request.Id);

        await _categories.DeleteAsync(category, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
