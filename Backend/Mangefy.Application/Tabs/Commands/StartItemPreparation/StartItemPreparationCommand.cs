using MediatR;

namespace Mangefy.Application.Tabs.Commands.StartItemPreparation;

public sealed record StartItemPreparationCommand(
    Guid TenantId,
    Guid TabId,
    Guid OrderId,
    Guid ItemId
) : IRequest;
