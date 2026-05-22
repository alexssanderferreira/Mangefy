using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Settings;
using Mangefy.Domain.Settings.Repositories;
using MediatR;

namespace Mangefy.Application.Settings.PrinterSettings.Commands.UpdatePrinter;

public sealed class UpdatePrinterCommandHandler : IRequestHandler<UpdatePrinterCommand>
{
    private readonly IPrinterSettingsRepository _repository;
    private readonly IUnitOfWork _uow;

    public UpdatePrinterCommandHandler(IPrinterSettingsRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(UpdatePrinterCommand request, CancellationToken cancellationToken)
    {
        var settings = await _repository.GetByTenantIdAsync(request.TenantId, cancellationToken)
            ?? throw new NotFoundException("Configuração de impressoras", request.TenantId);

        var station = Enum.Parse<PrinterStation>(request.Station);
        settings.UpdatePrinter(request.PrinterId, request.Name, request.IpAddressOrPort, station);

        await _repository.UpdateAsync(settings, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
