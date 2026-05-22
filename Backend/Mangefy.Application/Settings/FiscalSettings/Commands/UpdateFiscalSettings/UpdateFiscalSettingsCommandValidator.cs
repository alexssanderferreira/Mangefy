using FluentValidation;

namespace Mangefy.Application.Settings.FiscalSettings.Commands.UpdateFiscalSettings;

public sealed class UpdateFiscalSettingsCommandValidator : AbstractValidator<UpdateFiscalSettingsCommand>
{
    public UpdateFiscalSettingsCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        When(x => x.NfceEnabled, () =>
        {
            RuleFor(x => x.Cnpj).NotEmpty().WithMessage("CNPJ é obrigatório para habilitar NFC-e.");
            RuleFor(x => x.FiscalHubApiKey).NotEmpty().WithMessage("Chave de API do hub fiscal é obrigatória.");
        });
    }
}
