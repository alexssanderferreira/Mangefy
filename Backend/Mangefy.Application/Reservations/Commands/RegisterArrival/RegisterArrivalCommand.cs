using MediatR;

namespace Mangefy.Application.Reservations.Commands.RegisterArrival;

public sealed record RegisterArrivalCommand(
    Guid TenantId,
    Guid ReservationId,
    Guid EmployeeId
) : IRequest<Guid>;
