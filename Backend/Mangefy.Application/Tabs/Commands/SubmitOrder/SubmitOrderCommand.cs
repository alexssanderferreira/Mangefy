using Mangefy.Application.Common.Behaviors;
using MediatR;

namespace Mangefy.Application.Tabs.Commands.SubmitOrder;

public sealed record OrderItemRequest(
    Guid MenuItemId,
    int Quantity,
    string? Notes = null,
    IReadOnlyList<string>? Modifiers = null);

public sealed record SubmitOrderCommand(
    Guid TenantId,
    Guid TabId,
    Guid EmployeeId,
    IReadOnlyList<OrderItemRequest> Items,
    Guid? ClientCommandId = null
) : IRequest<Guid>, IIdempotentCommand;
