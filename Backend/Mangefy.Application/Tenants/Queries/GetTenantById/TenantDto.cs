using Mangefy.Domain.Tenants;

namespace Mangefy.Application.Tenants.Queries.GetTenantById;

public sealed record TenantDto(
    Guid Id,
    string Name,
    string Slug,
    string? Email,
    string? Phone,
    string Status,
    Guid PlanId,
    Guid BusinessTypeId,
    string Timezone,
    DateTime? TrialEndsAt,
    DateTime CreatedAt,
    AddressDto? Address
)
{
    public static TenantDto FromDomain(Tenant t) => new(
        t.Id, t.Name, t.Slug, t.Email?.Value,
        t.Phone?.Value,
        t.Status.ToString(), t.PlanId, t.BusinessTypeId,
        t.Timezone, t.TrialEndsAt, t.CreatedAt,
        t.Address is null ? null : new AddressDto(
            t.Address.Cep, t.Address.Logradouro, t.Address.Numero,
            t.Address.Complemento, t.Address.Bairro, t.Address.Cidade, t.Address.Uf));
}

public sealed record AddressDto(
    string Cep, string Logradouro, string Numero,
    string? Complemento, string Bairro, string Cidade, string Uf);
