using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Reservations.Repositories;
using MediatR;

namespace Mangefy.Application.Reservations.Commands.MarkNoShow;

public sealed class MarkNoShowCommandHandler : IRequestHandler<MarkNoShowCommand>
{
    private readonly IReservationRepository _reservations;
    private readonly IUnitOfWork _uow;

    public MarkNoShowCommandHandler(IReservationRepository reservations, IUnitOfWork uow)
    {
        _reservations = reservations;
        _uow = uow;
    }

    public async Task Handle(MarkNoShowCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _reservations.GetByIdAsync(request.ReservationId, cancellationToken)
            ?? throw new NotFoundException("Reserva", request.ReservationId);

        if (reservation.TenantId != request.TenantId)
            throw new ForbiddenException();

        reservation.MarkAsNoShow();
        await _reservations.UpdateAsync(reservation, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
