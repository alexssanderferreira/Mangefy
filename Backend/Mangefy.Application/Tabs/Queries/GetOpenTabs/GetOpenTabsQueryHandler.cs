using Mangefy.Domain.Tabs.Repositories;
using MediatR;

namespace Mangefy.Application.Tabs.Queries.GetOpenTabs;

public sealed class GetOpenTabsQueryHandler
    : IRequestHandler<GetOpenTabsQuery, IReadOnlyList<TabDto>>
{
    private readonly ITabRepository _tabs;

    public GetOpenTabsQueryHandler(ITabRepository tabs)
        => _tabs = tabs;

    public async Task<IReadOnlyList<TabDto>> Handle(
        GetOpenTabsQuery request, CancellationToken cancellationToken)
    {
        var tabs = await _tabs.GetOpenByTenantAsync(request.TenantId, cancellationToken);
        return tabs.Select(TabDto.FromDomain).ToList();
    }
}
