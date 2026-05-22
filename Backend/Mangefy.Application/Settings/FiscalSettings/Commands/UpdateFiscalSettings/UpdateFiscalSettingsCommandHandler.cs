using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Settings.Repositories;
using MediatR;
using DomainFiscalSettings = Mangefy.Domain.Settings.FiscalSettings;

namespace Mangefy.Application.Settings.FiscalSettings.Commands.UpdateFiscalSettings;

public sealed class UpdateFiscalSettingsCommandHandler : IRequestHandler<UpdateFiscalSettingsCommand>
{
    private readonly IFiscalSettingsRepository _repository;
    private readonly IUnitOfWork _uow;

    public UpdateFiscalSettingsCommandHandler(IFiscalSettingsRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(UpdateFiscalSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await _repository.GetByTenantIdAsync(request.TenantId, cancellationToken);

        if (settings is null)
        {
            settings = DomainFiscalSettings.CreateDefault(request.TenantId);
            await _repository.AddAsync(settings, cancellationToken);
        }

        if (request.NfceEnabled)
            settings.EnableNfce(request.Cnpj!, request.FiscalHubApiKey!, request.AutoEmitOnTabClose);
        else
            settings.DisableNfce();

        await _repository.UpdateAsync(settings, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
