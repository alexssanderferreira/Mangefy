using FluentValidation;

namespace Mangefy.Application.Auth.Commands.ActivateOwnerAccount;

public sealed class ActivateOwnerAccountCommandValidator : AbstractValidator<ActivateOwnerAccountCommand>
{
    public ActivateOwnerAccountCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8)
            .WithMessage("A senha deve ter no mínimo 8 caracteres.");
    }
}
