using Mangefy.Domain.Owners.Repositories;
using Mangefy.Domain.Tenants.Repositories;
using MediatR;

namespace Mangefy.Application.Owners.Queries.ListOwners;

public sealed class ListOwnersQueryHandler : IRequestHandler<ListOwnersQuery, ListOwnersResult>
{
    private readonly IOwnerRepository _owners;
    private readonly ITenantRepository _tenants;

    public ListOwnersQueryHandler(IOwnerRepository owners, ITenantRepository tenants)
    {
        _owners = owners;
        _tenants = tenants;
    }

    public async Task<ListOwnersResult> Handle(ListOwnersQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _owners.GetPagedAsync(request.Page, request.PageSize, cancellationToken);

        var dtos = new List<OwnerListItemDto>();
        foreach (var owner in items)
        {
            var count = await _tenants.CountByOwnerAsync(owner.Id, cancellationToken);
            dtos.Add(new OwnerListItemDto(
                owner.Id,
                owner.Name,
                owner.Email.Value,
                owner.Status.ToString(),
                count,
                owner.LastLoginAt,
                owner.CreatedAt));
        }

        return new ListOwnersResult(dtos, total, request.Page, request.PageSize);
    }
}
