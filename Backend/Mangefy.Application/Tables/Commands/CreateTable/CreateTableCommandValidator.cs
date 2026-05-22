using FluentValidation;

namespace Mangefy.Application.Tables.Commands.CreateTable;

public sealed class CreateTableCommandValidator : AbstractValidator<CreateTableCommand>
{
    public CreateTableCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Number).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Capacity).GreaterThan(0);
    }
}
