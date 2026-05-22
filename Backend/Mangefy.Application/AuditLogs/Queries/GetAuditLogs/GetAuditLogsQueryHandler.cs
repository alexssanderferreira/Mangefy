using Mangefy.Domain.Audit.Repositories;
using MediatR;

namespace Mangefy.Application.AuditLogs.Queries.GetAuditLogs;

public sealed class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogDto>>
{
    private readonly IAuditLogRepository _auditLogs;

    public GetAuditLogsQueryHandler(IAuditLogRepository auditLogs)
    {
        _auditLogs = auditLogs;
    }

    public async Task<IReadOnlyList<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _auditLogs.GetByTenantAsync(request.TenantId, request.From, request.To, cancellationToken);

        return logs.Select(l => new AuditLogDto(
            l.Id,
            l.EmployeeId,
            l.IsAdminSaas,
            l.Action,
            l.EntityType,
            l.EntityId,
            l.Reason,
            l.Before,
            l.After,
            l.OccurredAt))
        .ToList();
    }
}
