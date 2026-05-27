using MediatR;

namespace Mangefy.Application.Platform.PlatformSuppliers.Commands.UpdatePlatformSupplier;

public sealed record UpdatePlatformSupplierCommand(
    Guid SupplierId,
    string Name,
    Guid SupplierCategoryId,
    string? Cnpj,
    string? Website,
    string? Email,
    string? Phone,
    string? Description,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Bairro,
    string? Cidade,
    string? Uf,
    string? Complemento,
    string? BusinessHours) : IRequest;
