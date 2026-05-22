using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Employees;
using Mangefy.Domain.Employees.Repositories;
using MediatR;

namespace Mangefy.Application.Auth.Commands.SetPassword;

public sealed class SetPasswordCommandHandler : IRequestHandler<SetPasswordCommand>
{
    private readonly IActivationTokenRepository _activationTokens;
    private readonly IEmployeeRepository _employees;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _uow;

    public SetPasswordCommandHandler(
        IActivationTokenRepository activationTokens,
        IEmployeeRepository employees,
        IPasswordHasher passwordHasher,
        IUnitOfWork uow)
    {
        _activationTokens = activationTokens;
        _employees = employees;
        _passwordHasher = passwordHasher;
        _uow = uow;
    }

    public async Task Handle(SetPasswordCommand request, CancellationToken cancellationToken)
    {
        var activationToken = await _activationTokens.GetByTokenAsync(request.Token, cancellationToken)
            ?? throw new ForbiddenException("Token inválido ou expirado.");

        if (!activationToken.IsValid())
            throw new ForbiddenException("Token inválido ou expirado.");

        var employee = await _employees.GetByIdAsync(activationToken.EmployeeId, cancellationToken)
            ?? throw new NotFoundException(nameof(Employee), activationToken.EmployeeId);

        var hash = _passwordHasher.Hash(request.NewPassword);
        employee.ChangePassword(hash);

        if (employee.Status == EmployeeStatus.PendingActivation)
            employee.Activate();

        activationToken.MarkAsUsed();

        await _employees.UpdateAsync(employee, cancellationToken);
        await _activationTokens.UpdateAsync(activationToken, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
