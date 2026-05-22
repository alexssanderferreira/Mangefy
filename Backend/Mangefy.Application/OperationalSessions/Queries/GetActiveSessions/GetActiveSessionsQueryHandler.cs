using Mangefy.Domain.OperationalSessions.Repositories;
using MediatR;

namespace Mangefy.Application.OperationalSessions.Queries.GetActiveSessions;

public sealed class GetActiveSessionsQueryHandler : IRequestHandler<GetActiveSessionsQuery, IReadOnlyList<ActiveSessionDto>>
{
    private readonly IOperationalSessionRepository _sessions;

    public GetActiveSessionsQueryHandler(IOperationalSessionRepository sessions) => _sessions = sessions;

    public async Task<IReadOnlyList<ActiveSessionDto>> Handle(GetActiveSessionsQuery request, CancellationToken cancellationToken)
    {
        var list = await _sessions.GetActiveByTenantAsync(request.TenantId, cancellationToken);
        return list.Select(s => new ActiveSessionDto(
            s.Id, s.EmployeeId, s.DeviceId, s.StartedAt, s.IsWithinShift, s.HasTemporaryAccess)).ToList();
    }
}
