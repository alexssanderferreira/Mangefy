using Mangefy.Domain.Settings.Repositories;
using MediatR;

namespace Mangefy.Application.Settings.ReservationSettings.Queries.GetReservationSettings;

public sealed class GetReservationSettingsQueryHandler
    : IRequestHandler<GetReservationSettingsQuery, ReservationSettingsDto?>
{
    private readonly IReservationSettingsRepository _repository;

    public GetReservationSettingsQueryHandler(IReservationSettingsRepository repository)
        => _repository = repository;

    public async Task<ReservationSettingsDto?> Handle(
        GetReservationSettingsQuery request, CancellationToken cancellationToken)
    {
        var s = await _repository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (s is null) return null;
        return new ReservationSettingsDto(s.Id, s.MaxSimultaneousReservations);
    }
}
