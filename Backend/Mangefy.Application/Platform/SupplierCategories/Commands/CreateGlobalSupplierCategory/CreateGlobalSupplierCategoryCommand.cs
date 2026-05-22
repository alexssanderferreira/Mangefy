using MediatR;

namespace Mangefy.Application.Platform.SupplierCategories.Commands.CreateGlobalSupplierCategory;

public sealed record CreateGlobalSupplierCategoryCommand(string Name, string? Description = null) : IRequest<Guid>;
