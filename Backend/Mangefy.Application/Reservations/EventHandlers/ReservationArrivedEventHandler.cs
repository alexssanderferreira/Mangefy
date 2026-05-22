using Mangefy.Domain.Reservations.Events;
using MediatR;

namespace Mangefy.Application.Reservations.EventHandlers;

/// <summary>
/// Handler informativo do ReservationArrivedEvent.
/// A Tab já foi aberta antes deste evento ser disparado (em RegisterArrivalCommandHandler).
/// Reservado para futura integração: notificações, KDS, sinalização em tempo real.
/// </summary>
public sealed class ReservationArrivedEventHandler : INotificationHandler<ReservationArrivedEvent>
{
    public Task Handle(ReservationArrivedEvent notification, CancellationToken cancellationToken)
        => Task.CompletedTask;
}
