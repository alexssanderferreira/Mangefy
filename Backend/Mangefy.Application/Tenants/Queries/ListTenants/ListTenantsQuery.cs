using Mangefy.Application.Tenants.Queries.GetTenantById;
using MediatR;

namespace Mangefy.Application.Tenants.Queries.ListTenants;

public sealed record ListTenantsQuery(int Page = 1, int PageSize = 10) : IRequest<PagedResult<TenantDto>>;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
}
