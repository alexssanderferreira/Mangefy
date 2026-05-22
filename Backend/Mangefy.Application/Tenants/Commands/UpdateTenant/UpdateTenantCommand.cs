using MediatR;

namespace Mangefy.Application.Tenants.Commands.UpdateTenant;

public sealed record UpdateTenantCommand(
    Guid TenantId,
    string Name,
    string? LogoUrl,
    string? Email,
    string? Timezone,
    string? Phone,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Uf
) : IRequest;
