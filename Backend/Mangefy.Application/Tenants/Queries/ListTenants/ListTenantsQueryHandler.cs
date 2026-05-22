using Mangefy.Application.Tenants.Queries.GetTenantById;
using Mangefy.Domain.Tenants.Repositories;
using MediatR;

namespace Mangefy.Application.Tenants.Queries.ListTenants;

public sealed class ListTenantsQueryHandler : IRequestHandler<ListTenantsQuery, PagedResult<TenantDto>>
{
    private readonly ITenantRepository _tenants;

    public ListTenantsQueryHandler(ITenantRepository tenants) => _tenants = tenants;

    public async Task<PagedResult<TenantDto>> Handle(ListTenantsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _tenants.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
        return new PagedResult<TenantDto>(items.Select(TenantDto.FromDomain).ToList(), total, request.Page, request.PageSize);
    }
}
