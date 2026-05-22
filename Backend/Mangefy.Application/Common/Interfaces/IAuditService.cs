namespace Mangefy.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        Guid tenantId,
        Guid? employeeId,
        bool isAdminSaas,
        string action,
        string entityType,
        Guid entityId,
        string? reason = null,
        string? before = null,
        string? after = null,
        CancellationToken ct = default);
}
