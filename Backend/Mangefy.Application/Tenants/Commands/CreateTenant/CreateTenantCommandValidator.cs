using FluentValidation;

namespace Mangefy.Application.Tenants.Commands.CreateTenant;

public sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100)
            .Matches("^[a-z0-9-]+$").WithMessage("Slug deve conter apenas letras minúsculas, números e hifens.");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.PlanId).NotEmpty();
        RuleFor(x => x.BusinessTypeId).NotEmpty();
        RuleFor(x => x.Timezone).NotEmpty();
        RuleFor(x => x.TrialDays).GreaterThanOrEqualTo(0);
    }
}
