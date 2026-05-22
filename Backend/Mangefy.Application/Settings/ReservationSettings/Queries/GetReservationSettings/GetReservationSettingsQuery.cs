using MediatR;

namespace Mangefy.Application.Settings.ReservationSettings.Queries.GetReservationSettings;

public sealed record ReservationSettingsDto(Guid Id, int? MaxSimultaneousReservations);

public sealed record GetReservationSettingsQuery(Guid TenantId)
    : IRequest<ReservationSettingsDto?>;
