using MediatR;

namespace Mangefy.Application.Owners.Commands.UpdateOwner;

public sealed record UpdateOwnerCommand(
    Guid OwnerId,
    string Name,
    string Email,
    string? Phone,
    string? DocumentType,
    string? DocumentNumber,
    string? Notes,
    UpdateOwnerAddressDto? Address
) : IRequest;

public sealed record UpdateOwnerAddressDto(
    string Cep,
    string Logradouro,
    string Numero,
    string? Complemento,
    string Bairro,
    string Cidade,
    string Uf);
