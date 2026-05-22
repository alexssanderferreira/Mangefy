using FluentValidation;

namespace Mangefy.Application.DailyCash.Commands.OpenCashRegister;

public sealed class OpenCashRegisterCommandValidator : AbstractValidator<OpenCashRegisterCommand>
{
    public OpenCashRegisterCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.OpeningAmount).GreaterThanOrEqualTo(0);
    }
}
