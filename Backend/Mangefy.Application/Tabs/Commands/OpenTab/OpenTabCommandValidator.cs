using FluentValidation;

namespace Mangefy.Application.Tabs.Commands.OpenTab;

public sealed class OpenTabCommandValidator : AbstractValidator<OpenTabCommand>
{
    public OpenTabCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x).Must(x => x.TableId.HasValue || !string.IsNullOrWhiteSpace(x.LocationNote))
            .WithMessage("Informe a mesa ou uma localização para a comanda.");
    }
}
