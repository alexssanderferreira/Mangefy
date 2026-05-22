using MediatR;

namespace Mangefy.Application.Platform.SupplierCategories.Commands.UpdateSupplierCategory;

public sealed record UpdateSupplierCategoryCommand(Guid CategoryId, string Name, string? Description) : IRequest;
