using Mangefy.Application.Tabs.Queries.GetOpenTabs;
using MediatR;

namespace Mangefy.Application.Tabs.Queries.GetTabById;

public sealed record GetTabByIdQuery(Guid TenantId, Guid TabId)
    : IRequest<TabDto>;
