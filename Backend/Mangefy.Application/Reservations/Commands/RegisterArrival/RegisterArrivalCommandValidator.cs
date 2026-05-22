using FluentValidation;

namespace Mangefy.Application.Reservations.Commands.RegisterArrival;

public sealed class RegisterArrivalCommandValidator : AbstractValidator<RegisterArrivalCommand>
{
    public RegisterArrivalCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ReservationId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
    }
}
