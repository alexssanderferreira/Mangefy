using MediatR;

namespace Mangefy.Application.AuditLogs.Queries.GetAuditLogs;

public sealed record GetAuditLogsQuery(
    Guid TenantId,
    DateTime From,
    DateTime To
) : IRequest<IReadOnlyList<AuditLogDto>>;

public sealed record AuditLogDto(
    Guid Id,
    Guid? EmployeeId,
    bool IsAdminSaas,
    string Action,
    string EntityType,
    Guid EntityId,
    string? Reason,
    string? Before,
    string? After,
    DateTime OccurredAt);
