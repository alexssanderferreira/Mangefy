using MediatR;

namespace Mangefy.Application.Tabs.Commands.CancelOrderItem;

public sealed record CancelOrderItemCommand(
    Guid TenantId,
    Guid TabId,
    Guid OrderId,
    Guid ItemId,
    string? Reason
) : IRequest;
