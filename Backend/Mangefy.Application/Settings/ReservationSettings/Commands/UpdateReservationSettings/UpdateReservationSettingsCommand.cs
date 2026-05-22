using MediatR;

namespace Mangefy.Application.Settings.ReservationSettings.Commands.UpdateReservationSettings;

public sealed record UpdateReservationSettingsCommand(Guid TenantId, int? MaxSimultaneousReservations) : IRequest;
