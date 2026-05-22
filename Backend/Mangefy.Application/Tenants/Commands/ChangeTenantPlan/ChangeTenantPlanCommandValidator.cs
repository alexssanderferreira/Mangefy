using FluentValidation;

namespace Mangefy.Application.Tenants.Commands.ChangeTenantPlan;

public sealed class ChangeTenantPlanCommandValidator : AbstractValidator<ChangeTenantPlanCommand>
{
    public ChangeTenantPlanCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.NewPlanId).NotEmpty();
    }
}
