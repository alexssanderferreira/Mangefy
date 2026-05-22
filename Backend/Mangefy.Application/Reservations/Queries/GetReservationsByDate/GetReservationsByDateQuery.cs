using MediatR;

namespace Mangefy.Application.Reservations.Queries.GetReservationsByDate;

public sealed record GetReservationsByDateQuery(Guid TenantId, DateOnly Date)
    : IRequest<IReadOnlyList<ReservationDto>>;
