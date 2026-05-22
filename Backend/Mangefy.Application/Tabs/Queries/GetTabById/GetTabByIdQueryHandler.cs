using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Tabs.Queries.GetOpenTabs;
using Mangefy.Domain.Tabs.Repositories;
using MediatR;

namespace Mangefy.Application.Tabs.Queries.GetTabById;

public sealed class GetTabByIdQueryHandler
    : IRequestHandler<GetTabByIdQuery, TabDto>
{
    private readonly ITabRepository _tabs;

    public GetTabByIdQueryHandler(ITabRepository tabs)
        => _tabs = tabs;

    public async Task<TabDto> Handle(
        GetTabByIdQuery request, CancellationToken cancellationToken)
    {
        var tab = await _tabs.GetByIdAsync(request.TabId, cancellationToken)
            ?? throw new NotFoundException("Comanda", request.TabId);

        if (tab.TenantId != request.TenantId)
            throw new ForbiddenException();

        return TabDto.FromDomain(tab);
    }
}
