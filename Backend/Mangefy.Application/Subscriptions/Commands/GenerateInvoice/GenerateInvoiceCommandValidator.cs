using FluentValidation;

namespace Mangefy.Application.Subscriptions.Commands.GenerateInvoice;

public sealed class GenerateInvoiceCommandValidator : AbstractValidator<GenerateInvoiceCommand>
{
    public GenerateInvoiceCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.DueDate).Must(d => d >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Data de vencimento deve ser hoje ou no futuro.");
    }
}
