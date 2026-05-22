using Mangefy.Domain.Roles.Repositories;
using MediatR;

namespace Mangefy.Application.Roles.Queries.GetRolesByTenant;

public sealed class GetRolesByTenantQueryHandler
    : IRequestHandler<GetRolesByTenantQuery, IReadOnlyList<TenantRoleDto>>
{
    private readonly ITenantRoleRepository _roles;

    public GetRolesByTenantQueryHandler(ITenantRoleRepository roles)
        => _roles = roles;

    public async Task<IReadOnlyList<TenantRoleDto>> Handle(
        GetRolesByTenantQuery request, CancellationToken cancellationToken)
    {
        var roles = await _roles.GetByTenantAsync(request.TenantId, cancellationToken);
        return roles.Select(TenantRoleDto.FromDomain).ToList();
    }
}
