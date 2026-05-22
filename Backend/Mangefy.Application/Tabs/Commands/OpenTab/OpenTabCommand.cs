using Mangefy.Application.Common.Behaviors;
using Mangefy.Domain.Tabs;
using MediatR;

namespace Mangefy.Application.Tabs.Commands.OpenTab;

public sealed record OpenTabCommand(
    Guid TenantId,
    Guid EmployeeId,
    string CustomerName,
    Guid? TableId,
    string? LocationNote,
    SaleChannel Channel = SaleChannel.InPerson,
    DeliveryInfo? DeliveryInfo = null,
    Guid? ClientCommandId = null
) : IRequest<Guid>, IIdempotentCommand;
