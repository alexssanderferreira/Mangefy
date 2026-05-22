using MediatR;

namespace Mangefy.Application.Platform.SupplierCategories.Commands.DeleteSupplierCategory;

public sealed record DeleteSupplierCategoryCommand(Guid Id) : IRequest;
