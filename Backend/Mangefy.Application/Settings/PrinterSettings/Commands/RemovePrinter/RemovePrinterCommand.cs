using MediatR;

namespace Mangefy.Application.Settings.PrinterSettings.Commands.RemovePrinter;

public sealed record RemovePrinterCommand(Guid TenantId, Guid PrinterId) : IRequest;
