using Mangefy.Domain.Reservations;

namespace Mangefy.Application.Reservations.Queries.GetReservationsByDate;

public sealed record ReservationDto(
    Guid Id,
    string CustomerName,
    string? CustomerPhone,
    int PartySize,
    DateOnly Date,
    TimeOnly Time,
    Guid? TableId,
    string? Notes,
    string Status,
    Guid? TabId
)
{
    public static ReservationDto FromDomain(Reservation r) => new(
        r.Id,
        r.CustomerName,
        r.CustomerPhone?.Value,
        r.PartySize,
        r.Date,
        r.Time,
        r.TableId,
        r.Notes,
        r.Status.ToString(),
        r.TabId);
}
