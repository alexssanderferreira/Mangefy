using FluentValidation;
using Mangefy.Domain.Tabs;

namespace Mangefy.Application.Settings.PaymentSettings.Commands.UpdatePaymentSettings;

public sealed class UpdatePaymentSettingsCommandValidator : AbstractValidator<UpdatePaymentSettingsCommand>
{
    public UpdatePaymentSettingsCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EnabledMethods).NotEmpty()
            .WithMessage("Pelo menos um método de pagamento deve ser habilitado.");
        RuleForEach(x => x.EnabledMethods)
            .Must(m => Enum.TryParse<PaymentMethod>(m, out _))
            .WithMessage("Método de pagamento inválido.");
    }
}
