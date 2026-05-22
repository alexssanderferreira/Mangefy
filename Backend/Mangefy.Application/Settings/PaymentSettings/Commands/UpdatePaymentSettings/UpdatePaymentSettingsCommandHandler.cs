using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Settings.Repositories;
using Mangefy.Domain.Tabs;
using MediatR;
using DomainPaymentSettings = Mangefy.Domain.Settings.PaymentSettings;

namespace Mangefy.Application.Settings.PaymentSettings.Commands.UpdatePaymentSettings;

public sealed class UpdatePaymentSettingsCommandHandler : IRequestHandler<UpdatePaymentSettingsCommand>
{
    private readonly IPaymentSettingsRepository _repository;
    private readonly IUnitOfWork _uow;

    public UpdatePaymentSettingsCommandHandler(IPaymentSettingsRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(UpdatePaymentSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await _repository.GetByTenantIdAsync(request.TenantId, cancellationToken);

        if (settings is null)
        {
            settings = DomainPaymentSettings.CreateDefault(request.TenantId);
            await _repository.AddAsync(settings, cancellationToken);
        }

        var desired = request.EnabledMethods.Select(m => Enum.Parse<PaymentMethod>(m)).ToHashSet();
        var current = settings.EnabledMethods.ToHashSet();

        foreach (var method in current.Except(desired))
            settings.DisableMethod(method);

        foreach (var method in desired.Except(current))
            settings.EnableMethod(method);

        await _repository.UpdateAsync(settings, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
