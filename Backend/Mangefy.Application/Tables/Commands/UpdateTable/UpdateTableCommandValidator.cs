using FluentValidation;

namespace Mangefy.Application.Tables.Commands.UpdateTable;

public sealed class UpdateTableCommandValidator : AbstractValidator<UpdateTableCommand>
{
    public UpdateTableCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TableId).NotEmpty();
        RuleFor(x => x.Number).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Capacity).GreaterThan(0);
    }
}
