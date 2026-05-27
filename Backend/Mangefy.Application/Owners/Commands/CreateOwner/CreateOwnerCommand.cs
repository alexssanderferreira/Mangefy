using MediatR;

namespace Mangefy.Application.Owners.Commands.CreateOwner;

public sealed record CreateOwnerCommand(
    string Name,
    string Email,
    string? Phone,
    string? DocumentType,
    string? DocumentNumber,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Uf
) : IRequest<CreateOwnerResult>;

public sealed record CreateOwnerResult(Guid Id, string ActivationToken, DateTime ActivationTokenExpiresAt);
