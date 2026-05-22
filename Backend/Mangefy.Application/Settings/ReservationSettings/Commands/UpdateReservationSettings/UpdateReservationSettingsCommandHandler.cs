using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Settings.Repositories;
using MediatR;
using DomainReservationSettings = Mangefy.Domain.Settings.ReservationSettings;

namespace Mangefy.Application.Settings.ReservationSettings.Commands.UpdateReservationSettings;

public sealed class UpdateReservationSettingsCommandHandler : IRequestHandler<UpdateReservationSettingsCommand>
{
    private readonly IReservationSettingsRepository _repository;
    private readonly IUnitOfWork _uow;

    public UpdateReservationSettingsCommandHandler(IReservationSettingsRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(UpdateReservationSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await _repository.GetByTenantIdAsync(request.TenantId, cancellationToken);

        if (settings is null)
        {
            settings = DomainReservationSettings.CreateDefault(request.TenantId);
            await _repository.AddAsync(settings, cancellationToken);
        }

        settings.UpdateLimit(request.MaxSimultaneousReservations);
        await _repository.UpdateAsync(settings, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
