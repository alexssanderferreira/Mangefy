using MediatR;

namespace Mangefy.Application.OperationalSessions.Commands.StartSession;

public sealed record StartSessionCommand(
    Guid TenantId,
    Guid EmployeeId,
    Guid? DeviceId = null
) : IRequest<StartSessionResult>;

public sealed record StartSessionResult(Guid SessionId, bool IsWithinShift);
