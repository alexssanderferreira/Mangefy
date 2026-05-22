using Mangefy.Application.Common.Exceptions;
using Mangefy.Domain.Tenants;
using Mangefy.Domain.Tenants.Repositories;
using MediatR;

namespace Mangefy.Application.Tenants.Queries.GetTenantById;

public sealed class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, TenantDto>
{
    private readonly ITenantRepository _tenants;

    public GetTenantByIdQueryHandler(ITenantRepository tenants) => _tenants = tenants;

    public async Task<TenantDto> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _tenants.GetByIdAsync(request.TenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenant), request.TenantId);

        return TenantDto.FromDomain(tenant);
    }
}
