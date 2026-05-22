using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Common;
using Mangefy.Domain.Platform.Features;
using Mangefy.Domain.Reservations;
using Mangefy.Domain.Reservations.Repositories;
using Mangefy.Domain.Settings.Repositories;
using MediatR;

namespace Mangefy.Application.Reservations.Commands.CreateReservation;

public sealed class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, Guid>
{
    private readonly IReservationRepository _reservations;
    private readonly IReservationSettingsRepository _reservationSettings;
    private readonly IFeatureGateService _featureGate;
    private readonly IUnitOfWork _uow;

    public CreateReservationCommandHandler(
        IReservationRepository reservations,
        IReservationSettingsRepository reservationSettings,
        IFeatureGateService featureGate,
        IUnitOfWork uow)
    {
        _reservations = reservations;
        _reservationSettings = reservationSettings;
        _featureGate = featureGate;
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        await _featureGate.RequireAsync(request.TenantId, FeatureCatalog.Reservations.Management, cancellationToken);

        // Verificar limite de reservas simultâneas (mesma data e horário)
        var settings = await _reservationSettings.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (settings?.MaxSimultaneousReservations is int maxSimultaneous)
        {
            var sameDay = await _reservations.GetByDateAsync(request.TenantId, request.Date, cancellationToken);
            var simultaneousCount = sameDay.Count(r =>
                r.Time == request.Time &&
                r.Status is not (ReservationStatus.Cancelled or ReservationStatus.NoShow));

            if (simultaneousCount >= maxSimultaneous)
                throw new DomainException(
                    $"Limite de {maxSimultaneous} reserva(s) simultâneas para o mesmo horário atingido.");
        }

        var reservation = Reservation.Create(
            request.TenantId, request.CustomerName, request.PartySize,
            request.Date, request.Time, request.EmployeeId,
            request.TableId, request.CustomerPhone, request.Notes);

        await _reservations.AddAsync(reservation, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return reservation.Id;
    }
}
