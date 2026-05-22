using MediatR;

namespace Mangefy.Application.Settings.PrinterSettings.Commands.AddPrinter;

public sealed record AddPrinterCommand(
    Guid TenantId,
    string Name,
    string IpAddressOrPort,
    string Station) : IRequest;
