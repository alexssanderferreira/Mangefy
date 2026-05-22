using Mangefy.Application.Common.Behaviors;
using MediatR;

namespace Mangefy.Application.Tabs.Commands.CancelTab;

public sealed record CancelTabCommand(
    Guid TenantId,
    Guid TabId,
    string Reason,
    Guid? ClientCommandId = null
) : IRequest, IIdempotentCommand;
