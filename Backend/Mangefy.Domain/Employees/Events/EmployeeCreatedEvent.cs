using Mangefy.Domain.Common;

namespace Mangefy.Domain.Employees.Events;

public sealed class EmployeeCreatedEvent(Guid employeeId, Guid tenantId, string email) : DomainEvent
{
    public Guid EmployeeId { get; } = employeeId;
    public Guid TenantId { get; } = tenantId;
    public string Email { get; } = email;
}
