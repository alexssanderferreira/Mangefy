using MediatR;

namespace Mangefy.Application.Platform.PlatformSuppliers.Commands.CreatePlatformSupplier;

public sealed record CreatePlatformSupplierCommand(
    string Name,
    Guid SupplierCategoryId,
    string? Cnpj = null,
    string? Website = null,
    string? Email = null,
    string? Phone = null,
    string? Description = null,
    string? Cep = null,
    string? Logradouro = null,
    string? Numero = null,
    string? Bairro = null,
    string? Cidade = null,
    string? Uf = null,
    string? Complemento = null,
    string? BusinessHours = null) : IRequest<Guid>;
