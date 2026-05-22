using Mangefy.Domain.Common;

namespace Mangefy.Domain.Reservations.Events;

public sealed class ReservationArrivedEvent : DomainEvent
{
    public Guid ReservationId { get; }
    public Guid TenantId { get; }
    public Guid TabId { get; }

    public ReservationArrivedEvent(Guid reservationId, Guid tenantId, Guid tabId)
    {
        ReservationId = reservationId;
        TenantId = tenantId;
        TabId = tabId;
    }
}
