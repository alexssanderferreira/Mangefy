using MediatR;

namespace Mangefy.Application.OperationalSessions.Queries.GetActiveSessions;

public sealed record GetActiveSessionsQuery(Guid TenantId) : IRequest<IReadOnlyList<ActiveSessionDto>>;

public sealed record ActiveSessionDto(
    Guid Id,
    Guid EmployeeId,
    Guid? DeviceId,
    DateTime StartedAt,
    bool IsWithinShift,
    bool HasTemporaryAccess);
