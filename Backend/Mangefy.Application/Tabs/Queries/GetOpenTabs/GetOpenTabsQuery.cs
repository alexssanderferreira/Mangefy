using MediatR;

namespace Mangefy.Application.Tabs.Queries.GetOpenTabs;

public sealed record GetOpenTabsQuery(Guid TenantId)
    : IRequest<IReadOnlyList<TabDto>>;
