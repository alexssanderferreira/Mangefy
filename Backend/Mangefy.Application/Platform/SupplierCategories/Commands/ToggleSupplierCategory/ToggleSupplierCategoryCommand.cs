using MediatR;

namespace Mangefy.Application.Platform.SupplierCategories.Commands.ToggleSupplierCategory;

public sealed record ToggleSupplierCategoryCommand(Guid Id, bool Activate) : IRequest;
