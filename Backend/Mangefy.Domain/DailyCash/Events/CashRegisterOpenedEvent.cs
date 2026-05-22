using Mangefy.Domain.Common;

namespace Mangefy.Domain.DailyCash.Events;

public sealed class CashRegisterOpenedEvent : DomainEvent
{
    public Guid CashRegisterId { get; }
    public Guid TenantId { get; }
    public Guid EmployeeId { get; }

    public CashRegisterOpenedEvent(Guid cashRegisterId, Guid tenantId, Guid employeeId)
    {
        CashRegisterId = cashRegisterId;
        TenantId = tenantId;
        EmployeeId = employeeId;
    }
}
