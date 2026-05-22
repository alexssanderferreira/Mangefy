using FluentValidation;
using Mangefy.Domain.Settings;

namespace Mangefy.Application.Settings.PrinterSettings.Commands.AddPrinter;

public sealed class AddPrinterCommandValidator : AbstractValidator<AddPrinterCommand>
{
    public AddPrinterCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.IpAddressOrPort).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Station).Must(s => Enum.TryParse<PrinterStation>(s, out _))
            .WithMessage("Estação inválida. Use Cashier, Kitchen, Bar ou Custom.");
    }
}
