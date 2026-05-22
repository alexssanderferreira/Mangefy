using MediatR;

namespace Mangefy.Application.Reservations.Commands.MarkNoShow;

public sealed record MarkNoShowCommand(Guid TenantId, Guid ReservationId) : IRequest;
