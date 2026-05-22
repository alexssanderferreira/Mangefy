using Mangefy.Domain.Settings.Repositories;
using MediatR;

namespace Mangefy.Application.Settings.PrinterSettings.Queries.GetPrinterSettings;

public sealed class GetPrinterSettingsQueryHandler
    : IRequestHandler<GetPrinterSettingsQuery, PrinterSettingsDto?>
{
    private readonly IPrinterSettingsRepository _repository;

    public GetPrinterSettingsQueryHandler(IPrinterSettingsRepository repository)
        => _repository = repository;

    public async Task<PrinterSettingsDto?> Handle(
        GetPrinterSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _repository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (settings is null) return null;

        var printers = settings.Printers.Select(p =>
            new PrinterDto(p.Id, p.Name, p.IpAddressOrPort, p.Station.ToString(), p.IsDefault, p.IsActive))
            .ToList();

        return new PrinterSettingsDto(settings.Id, printers);
    }
}
