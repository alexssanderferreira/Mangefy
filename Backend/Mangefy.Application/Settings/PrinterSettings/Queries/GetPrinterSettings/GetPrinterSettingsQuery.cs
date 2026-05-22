using MediatR;

namespace Mangefy.Application.Settings.PrinterSettings.Queries.GetPrinterSettings;

public sealed record PrinterDto(
    Guid Id, string Name, string IpAddressOrPort, string Station, bool IsDefault, bool IsActive);

public sealed record PrinterSettingsDto(Guid Id, IReadOnlyList<PrinterDto> Printers);

public sealed record GetPrinterSettingsQuery(Guid TenantId)
    : IRequest<PrinterSettingsDto?>;
