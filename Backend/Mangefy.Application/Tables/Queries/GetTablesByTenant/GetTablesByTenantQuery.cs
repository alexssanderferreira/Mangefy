using MediatR;

namespace Mangefy.Application.Tables.Queries.GetTablesByTenant;

public sealed record GetTablesByTenantQuery(Guid TenantId)
    : IRequest<IReadOnlyList<TableDto>>;
