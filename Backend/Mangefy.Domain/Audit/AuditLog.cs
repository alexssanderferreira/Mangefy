using Mangefy.Domain.Common;

namespace Mangefy.Domain.Audit;

public sealed class AuditLog : Entity
{
    public Guid TenantId { get; private set; }
    public Guid? EmployeeId { get; private set; }
    public bool IsAdminSaas { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string? Reason { get; private set; }
    public string? Before { get; private set; }
    public string? After { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private AuditLog() { }

    public static AuditLog Record(
        Guid tenantId,
        Guid? employeeId,
        bool isAdminSaas,
        string action,
        string entityType,
        Guid entityId,
        string? reason = null,
        string? before = null,
        string? after = null)
    {
        return new AuditLog
        {
            TenantId = tenantId,
            EmployeeId = employeeId,
            IsAdminSaas = isAdminSaas,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Reason = reason,
            Before = before,
            After = after,
            OccurredAt = DateTime.UtcNow
        };
    }
}
