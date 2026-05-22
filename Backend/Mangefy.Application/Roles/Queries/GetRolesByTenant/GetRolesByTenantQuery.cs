using MediatR;

namespace Mangefy.Application.Roles.Queries.GetRolesByTenant;

public sealed record GetRolesByTenantQuery(Guid TenantId)
    : IRequest<IReadOnlyList<TenantRoleDto>>;
