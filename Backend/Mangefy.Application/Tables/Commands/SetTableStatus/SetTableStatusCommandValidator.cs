using FluentValidation;

namespace Mangefy.Application.Tables.Commands.SetTableStatus;

public sealed class SetTableStatusCommandValidator : AbstractValidator<SetTableStatusCommand>
{
    private static readonly string[] AllowedStatuses = ["Available", "Unavailable"];

    public SetTableStatusCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TableId).NotEmpty();
        RuleFor(x => x.Status).Must(s => AllowedStatuses.Contains(s))
            .WithMessage("Status inválido. Use Available ou Unavailable.");
    }
}
