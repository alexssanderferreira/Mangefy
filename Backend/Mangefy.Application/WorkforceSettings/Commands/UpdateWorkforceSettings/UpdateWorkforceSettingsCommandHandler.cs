using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Settings.Repositories;
using MediatR;
using DomainWorkforceSettings = Mangefy.Domain.Settings.WorkforceSettings;

namespace Mangefy.Application.WorkforceSettings.Commands.UpdateWorkforceSettings;

public sealed class UpdateWorkforceSettingsCommandHandler : IRequestHandler<UpdateWorkforceSettingsCommand>
{
    private readonly IWorkforceSettingsRepository _repository;
    private readonly IUnitOfWork _uow;

    public UpdateWorkforceSettingsCommandHandler(IWorkforceSettingsRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(UpdateWorkforceSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await _repository.GetByTenantIdAsync(request.TenantId, cancellationToken);

        if (settings is null)
        {
            settings = DomainWorkforceSettings.CreateDefault(request.TenantId);
            settings.UpdateShiftTolerance(request.ShiftToleranceMinutes);
            await _repository.AddAsync(settings, cancellationToken);
        }
        else
        {
            settings.UpdateShiftTolerance(request.ShiftToleranceMinutes);
            await _repository.UpdateAsync(settings, cancellationToken);
        }

        await _uow.SaveChangesAsync(cancellationToken);
    }
}
