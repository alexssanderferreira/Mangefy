using MediatR;

namespace Mangefy.Application.Settings.PrinterSettings.Commands.UpdatePrinter;

public sealed record UpdatePrinterCommand(
    Guid TenantId,
    Guid PrinterId,
    string Name,
    string IpAddressOrPort,
    string Station) : IRequest;
