using FluentValidation;

namespace Mangefy.Application.Auth.Commands.AdminSaasLogin;

public sealed class AdminSaasLoginCommandValidator : AbstractValidator<AdminSaasLoginCommand>
{
    public AdminSaasLoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
