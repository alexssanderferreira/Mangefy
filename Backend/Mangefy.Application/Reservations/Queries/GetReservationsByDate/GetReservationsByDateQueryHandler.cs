using Mangefy.Domain.Reservations.Repositories;
using MediatR;

namespace Mangefy.Application.Reservations.Queries.GetReservationsByDate;

public sealed class GetReservationsByDateQueryHandler
    : IRequestHandler<GetReservationsByDateQuery, IReadOnlyList<ReservationDto>>
{
    private readonly IReservationRepository _reservations;

    public GetReservationsByDateQueryHandler(IReservationRepository reservations)
        => _reservations = reservations;

    public async Task<IReadOnlyList<ReservationDto>> Handle(
        GetReservationsByDateQuery request, CancellationToken cancellationToken)
    {
        var reservations = await _reservations.GetByDateAsync(request.TenantId, request.Date, cancellationToken);
        return reservations.Select(ReservationDto.FromDomain).ToList();
    }
}
