using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Application.Tabs.Commands.OpenTab;
using Mangefy.Domain.Common;
using Mangefy.Domain.Reservations;
using Mangefy.Domain.Reservations.Repositories;
using Mangefy.Domain.Tabs;
using MediatR;

namespace Mangefy.Application.Reservations.Commands.RegisterArrival;

/// <summary>
/// Registra chegada do cliente e abre Tab automaticamente conforme CA-182/183.
/// Retorna o Id da Tab criada.
/// Decisão: reserva sem mesa usa LocationNote = "Reserva" como fallback (CA-179).
/// </summary>
public sealed class RegisterArrivalCommandHandler : IRequestHandler<RegisterArrivalCommand, Guid>
{
    private readonly IReservationRepository _reservations;
    private readonly ISender _sender;
    private readonly IUnitOfWork _uow;

    public RegisterArrivalCommandHandler(
        IReservationRepository reservations,
        ISender sender,
        IUnitOfWork uow)
    {
        _reservations = reservations;
        _sender = sender;
        _uow = uow;
    }

    public async Task<Guid> Handle(RegisterArrivalCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _reservations.GetByIdAsync(request.ReservationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), request.ReservationId);

        if (reservation.TenantId != request.TenantId)
            throw new ForbiddenException();

        // Valida status ANTES de abrir Tab para evitar Tab órfã (CA-186)
        if (reservation.Status is not (ReservationStatus.Pending or ReservationStatus.Confirmed))
            throw new DomainException(
                $"Chegada não pode ser registrada para reserva com status '{reservation.Status}'.");

        // Fallback de localização para reservas sem mesa pré-definida (CA-179)
        var locationNote = reservation.TableId is null ? "Reserva" : null;

        var openTabCommand = new OpenTabCommand(
            TenantId: request.TenantId,
            EmployeeId: request.EmployeeId,
            CustomerName: reservation.CustomerName,
            TableId: reservation.TableId,
            LocationNote: locationNote,
            Channel: SaleChannel.InPerson,
            DeliveryInfo: null,
            ClientCommandId: null);

        var tabId = await _sender.Send(openTabCommand, cancellationToken);

        reservation.RegisterArrival(tabId);
        await _reservations.UpdateAsync(reservation, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return tabId;
    }
}
