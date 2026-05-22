using MediatR;

namespace Mangefy.Application.Tabs.Commands.ReturnOrderItem;

public sealed record ReturnOrderItemCommand(
    Guid TenantId,
    Guid TabId,
    Guid OrderId,
    Guid ItemId
) : IRequest;
