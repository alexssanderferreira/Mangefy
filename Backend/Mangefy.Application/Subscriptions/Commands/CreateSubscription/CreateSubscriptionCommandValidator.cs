using FluentValidation;

namespace Mangefy.Application.Subscriptions.Commands.CreateSubscription;

public sealed class CreateSubscriptionCommandValidator : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PlanId).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.NextDueDate).GreaterThan(x => x.StartDate)
            .WithMessage("A data de vencimento deve ser posterior à data de início.");
    }
}
