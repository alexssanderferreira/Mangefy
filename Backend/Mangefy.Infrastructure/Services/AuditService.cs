using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Audit;
using Mangefy.Domain.Audit.Repositories;

namespace Mangefy.Infrastructure.Services;

public sealed class AuditService : IAuditService
{
    private readonly IAuditLogRepository _repository;

    public AuditService(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task LogAsync(
        Guid tenantId,
        Guid? employeeId,
        bool isAdminSaas,
        string action,
        string entityType,
        Guid entityId,
        string? reason = null,
        string? before = null,
        string? after = null,
        CancellationToken ct = default)
    {
        var log = AuditLog.Record(tenantId, employeeId, isAdminSaas, action, entityType, entityId, reason, before, after);
        await _repository.AddAsync(log, ct);
    }
}
