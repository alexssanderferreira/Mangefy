using MediatR;

namespace Mangefy.Application.Tenants.Queries.GetTenantById;

public sealed record GetTenantByIdQuery(Guid TenantId) : IRequest<TenantDto>;
