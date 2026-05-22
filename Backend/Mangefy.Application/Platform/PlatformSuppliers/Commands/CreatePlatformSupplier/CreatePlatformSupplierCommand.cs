using MediatR;

namespace Mangefy.Application.Platform.PlatformSuppliers.Commands.CreatePlatformSupplier;

public sealed record CreatePlatformSupplierCommand(
    string Name,
    Guid SupplierCategoryId,
    string? Cnpj = null,
    string? Website = null,
    string? Email = null,
    string? Phone = null,
    string? Description = null) : IRequest<Guid>;
