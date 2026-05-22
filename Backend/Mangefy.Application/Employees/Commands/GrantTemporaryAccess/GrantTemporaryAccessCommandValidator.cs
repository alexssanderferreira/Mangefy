using FluentValidation;

namespace Mangefy.Application.Employees.Commands.GrantTemporaryAccess;

public sealed class GrantTemporaryAccessCommandValidator : AbstractValidator<GrantTemporaryAccessCommand>
{
    public GrantTemporaryAccessCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.ExtensionMinutes).GreaterThan(0).LessThanOrEqualTo(480);
    }
}
