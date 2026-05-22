using MediatR;

namespace Mangefy.Application.OperationalSessions.Commands.EndSession;

public sealed record EndSessionCommand(Guid TenantId, Guid SessionId) : IRequest;
