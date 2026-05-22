using Mangefy.Domain.Platform.BusinessTypes.Repositories;
using Mangefy.Domain.Roles.Repositories;
using Mangefy.Domain.Tenants.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.BusinessTypes.Queries.GetBusinessTypes;

public sealed class GetBusinessTypesQueryHandler : IRequestHandler<GetBusinessTypesQuery, IReadOnlyList<BusinessTypeDto>>
{
    private readonly IBusinessTypeRepository _businessTypes;
    private readonly ITenantRepository _tenants;
    private readonly ITenantRoleRepository _tenantRoles;

    public GetBusinessTypesQueryHandler(
        IBusinessTypeRepository businessTypes,
        ITenantRepository tenants,
        ITenantRoleRepository tenantRoles)
    {
        _businessTypes = businessTypes;
        _tenants = tenants;
        _tenantRoles = tenantRoles;
    }

    public async Task<IReadOnlyList<BusinessTypeDto>> Handle(GetBusinessTypesQuery request, CancellationToken cancellationToken)
    {
        var list = await _businessTypes.GetAllAsync(cancellationToken);
        var tenantCounts = await _tenants.CountByBusinessTypeAsync(cancellationToken);
        var templateUsage = await _tenantRoles.CountByTemplateIdAsync(cancellationToken);

        return list.Select(bt => new BusinessTypeDto(
            bt.Id, bt.Name, bt.Description, bt.IsActive,
            bt.RoleTemplates.Select(t => new RoleTemplateDto(
                t.Id, t.Name, t.Description, t.IsActive, t.Permissions,
                templateUsage.GetValueOrDefault(t.Id, 0)
            )).ToList(),
            tenantCounts.GetValueOrDefault(bt.Id, 0)
        )).ToList();
    }
}
