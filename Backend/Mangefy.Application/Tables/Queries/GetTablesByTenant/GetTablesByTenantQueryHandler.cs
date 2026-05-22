using Mangefy.Domain.Tables.Repositories;
using MediatR;

namespace Mangefy.Application.Tables.Queries.GetTablesByTenant;

public sealed class GetTablesByTenantQueryHandler
    : IRequestHandler<GetTablesByTenantQuery, IReadOnlyList<TableDto>>
{
    private readonly ITableRepository _tables;

    public GetTablesByTenantQueryHandler(ITableRepository tables)
        => _tables = tables;

    public async Task<IReadOnlyList<TableDto>> Handle(
        GetTablesByTenantQuery request, CancellationToken cancellationToken)
    {
        var tables = await _tables.GetByTenantAsync(request.TenantId, cancellationToken);
        return tables.Select(TableDto.FromDomain).ToList();
    }
}
