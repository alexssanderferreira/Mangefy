using MediatR;

namespace Mangefy.Application.Reservations.Commands.CreateReservation;

public sealed record CreateReservationCommand(
    Guid TenantId,
    Guid EmployeeId,
    string CustomerName,
    string? CustomerPhone,
    int PartySize,
    DateOnly Date,
    TimeOnly Time,
    Guid? TableId,
    string? Notes
) : IRequest<Guid>;
