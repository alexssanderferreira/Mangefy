using MediatR;

namespace Mangefy.Application.Tabs.Commands.MarkItemReady;

public sealed record MarkItemReadyCommand(
    Guid TenantId,
    Guid TabId,
    Guid OrderId,
    Guid ItemId
) : IRequest;
