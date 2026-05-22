using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.DailyCash.Repositories;
using MediatR;

namespace Mangefy.Application.DailyCash.Commands.RegisterWithdrawal;

public sealed class RegisterWithdrawalCommandHandler : IRequestHandler<RegisterWithdrawalCommand>
{
    private readonly ICashRegisterRepository _cashRegisters;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _uow;

    public RegisterWithdrawalCommandHandler(
        ICashRegisterRepository cashRegisters, ICurrentUser currentUser, IUnitOfWork uow)
    {
        _cashRegisters = cashRegisters;
        _currentUser = currentUser;
        _uow = uow;
    }

    public async Task Handle(RegisterWithdrawalCommand request, CancellationToken cancellationToken)
    {
        var register = await _cashRegisters.GetOpenByTenantAsync(request.TenantId, cancellationToken)
            ?? throw new NotFoundException("Caixa aberto", request.TenantId);

        register.RegisterWithdrawal(request.Amount, request.Reason, _currentUser.EmployeeId ?? Guid.Empty);
        await _cashRegisters.UpdateAsync(register, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
