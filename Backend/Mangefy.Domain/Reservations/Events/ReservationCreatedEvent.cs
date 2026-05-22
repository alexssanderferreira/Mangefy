using Mangefy.Domain.Common;

namespace Mangefy.Domain.Reservations.Events;

public sealed class ReservationCreatedEvent : DomainEvent
{
    public Guid ReservationId { get; }
    public Guid TenantId { get; }
    public DateOnly Date { get; }
    public TimeOnly Time { get; }
    public Guid? TableId { get; }

    public ReservationCreatedEvent(Guid reservationId, Guid tenantId, DateOnly date, TimeOnly time, Guid? tableId)
    {
        ReservationId = reservationId;
        TenantId = tenantId;
        Date = date;
        Time = time;
        TableId = tableId;
    }
}
