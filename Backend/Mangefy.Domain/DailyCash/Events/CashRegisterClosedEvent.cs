using Mangefy.Domain.Common;

namespace Mangefy.Domain.DailyCash.Events;

public sealed class CashRegisterClosedEvent : DomainEvent
{
    public Guid CashRegisterId { get; }
    public Guid TenantId { get; }
    public decimal ExpectedAmount { get; }
    public decimal CountedAmount { get; }
    public decimal Difference { get; }
    public Guid EmployeeId { get; }

    public CashRegisterClosedEvent(
        Guid cashRegisterId, Guid tenantId, decimal expectedAmount, decimal countedAmount, Guid employeeId)
    {
        CashRegisterId = cashRegisterId;
        TenantId = tenantId;
        ExpectedAmount = expectedAmount;
        CountedAmount = countedAmount;
        Difference = countedAmount - expectedAmount;
        EmployeeId = employeeId;
    }
}
