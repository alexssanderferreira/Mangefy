using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Settings.Repositories;
using MediatR;
using DomainTabSettings = Mangefy.Domain.Settings.TabSettings;

namespace Mangefy.Application.Settings.TabSettings.Commands.UpdateTabSettings;

public sealed class UpdateTabSettingsCommandHandler : IRequestHandler<UpdateTabSettingsCommand>
{
    private readonly ITabSettingsRepository _repository;
    private readonly IUnitOfWork _uow;

    public UpdateTabSettingsCommandHandler(ITabSettingsRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(UpdateTabSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await _repository.GetByTenantIdAsync(request.TenantId, cancellationToken);

        if (settings is null)
        {
            settings = DomainTabSettings.CreateDefault(request.TenantId);
            await _repository.AddAsync(settings, cancellationToken);
        }

        settings.UpdateRange(request.MinTabNumber, request.MaxTabNumber);
        settings.UpdateDiscountPolicy(request.MaxDiscountPercent, request.DiscountReasonRequiredAbove);
        await _repository.UpdateAsync(settings, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
