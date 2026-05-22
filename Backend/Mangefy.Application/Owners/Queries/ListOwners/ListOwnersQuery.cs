using MediatR;

namespace Mangefy.Application.Owners.Queries.ListOwners;

public sealed record ListOwnersQuery(int Page = 1, int PageSize = 10) : IRequest<ListOwnersResult>;

public sealed record ListOwnersResult(IReadOnlyList<OwnerListItemDto> Items, int Total, int Page, int PageSize);

public sealed record OwnerListItemDto(
    Guid Id,
    string Name,
    string Email,
    string Status,
    int TenantCount,
    DateTime? LastLoginAt,
    DateTime CreatedAt);
