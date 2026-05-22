using MediatR;

namespace Mangefy.Application.Reservations.Commands.ConfirmReservation;

public sealed record ConfirmReservationCommand(Guid TenantId, Guid ReservationId) : IRequest;
