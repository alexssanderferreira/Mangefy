using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Settings;
using Mangefy.Domain.Settings.Repositories;
using MediatR;
using DomainPrinterSettings = Mangefy.Domain.Settings.PrinterSettings;

namespace Mangefy.Application.Settings.PrinterSettings.Commands.AddPrinter;

public sealed class AddPrinterCommandHandler : IRequestHandler<AddPrinterCommand>
{
    private readonly IPrinterSettingsRepository _repository;
    private readonly IUnitOfWork _uow;

    public AddPrinterCommandHandler(IPrinterSettingsRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(AddPrinterCommand request, CancellationToken cancellationToken)
    {
        var settings = await _repository.GetByTenantIdAsync(request.TenantId, cancellationToken);

        if (settings is null)
        {
            settings = DomainPrinterSettings.Create(request.TenantId);
            await _repository.AddAsync(settings, cancellationToken);
        }

        var station = Enum.Parse<PrinterStation>(request.Station);
        settings.AddPrinter(request.Name, request.IpAddressOrPort, station);

        await _repository.UpdateAsync(settings, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
