using MediatR;

namespace Mangefy.Application.Owners.Queries.GetOwnerById;

public sealed record GetOwnerByIdQuery(Guid OwnerId) : IRequest<OwnerDetailDto>;

public sealed record OwnerDetailDto(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? DocumentType,
    string? DocumentNumber,
    string? Notes,
    OwnerAddressDto? Address,
    string Status,
    DateTime? LastLoginAt,
    DateTime CreatedAt,
    OwnerMetricsDto Metrics,
    IReadOnlyList<OwnerTenantDto> Tenants);

public sealed record OwnerAddressDto(
    string Cep,
    string Logradouro,
    string Numero,
    string? Complemento,
    string Bairro,
    string Cidade,
    string Uf);

public sealed record OwnerMetricsDto(
    int TotalEstablishments,
    int ActiveEstablishments,
    int TrialEstablishments,
    int SuspendedEstablishments,
    IReadOnlyList<string> Plans,
    decimal EstimatedMrr,
    int DaysAsClient);

public sealed record OwnerTenantDto(
    Guid TenantId,
    string TenantName,
    string TenantSlug,
    string Status,
    string? PlanName,
    decimal? PlanPrice);
