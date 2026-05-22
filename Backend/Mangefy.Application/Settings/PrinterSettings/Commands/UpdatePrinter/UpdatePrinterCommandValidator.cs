using FluentValidation;
using Mangefy.Domain.Settings;

namespace Mangefy.Application.Settings.PrinterSettings.Commands.UpdatePrinter;

public sealed class UpdatePrinterCommandValidator : AbstractValidator<UpdatePrinterCommand>
{
    public UpdatePrinterCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PrinterId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.IpAddressOrPort).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Station).Must(s => Enum.TryParse<PrinterStation>(s, out _))
            .WithMessage("Estação inválida. Use Cashier, Kitchen, Bar ou Custom.");
    }
}
