using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Platform.SupplierCategories.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.SupplierCategories.Commands.ToggleSupplierCategory;

public sealed class ToggleSupplierCategoryCommandHandler : IRequestHandler<ToggleSupplierCategoryCommand>
{
    private readonly ISupplierCategoryRepository _categories;
    private readonly IUnitOfWork _uow;

    public ToggleSupplierCategoryCommandHandler(ISupplierCategoryRepository categories, IUnitOfWork uow)
    {
        _categories = categories;
        _uow = uow;
    }

    public async Task Handle(ToggleSupplierCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categories.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("SupplierCategory", request.Id);

        if (request.Activate) category.Activate();
        else category.Deactivate();

        await _categories.UpdateAsync(category, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
