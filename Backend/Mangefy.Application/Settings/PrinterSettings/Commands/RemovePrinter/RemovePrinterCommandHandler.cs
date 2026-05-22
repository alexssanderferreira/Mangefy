using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Settings.Repositories;
using MediatR;

namespace Mangefy.Application.Settings.PrinterSettings.Commands.RemovePrinter;

public sealed class RemovePrinterCommandHandler : IRequestHandler<RemovePrinterCommand>
{
    private readonly IPrinterSettingsRepository _repository;
    private readonly IUnitOfWork _uow;

    public RemovePrinterCommandHandler(IPrinterSettingsRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task Handle(RemovePrinterCommand request, CancellationToken cancellationToken)
    {
        var settings = await _repository.GetByTenantIdAsync(request.TenantId, cancellationToken)
            ?? throw new NotFoundException("Configuração de impressoras", request.TenantId);

        settings.RemovePrinter(request.PrinterId);
        await _repository.UpdateAsync(settings, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
