using MediatR;

namespace Mangefy.Application.Reservations.Commands.CancelReservation;

public sealed record CancelReservationCommand(Guid TenantId, Guid ReservationId, string Reason) : IRequest;
