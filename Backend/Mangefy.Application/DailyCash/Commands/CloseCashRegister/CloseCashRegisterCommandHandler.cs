using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.DailyCash;
using Mangefy.Domain.DailyCash.Repositories;
using MediatR;

namespace Mangefy.Application.DailyCash.Commands.CloseCashRegister;

public sealed class CloseCashRegisterCommandHandler : IRequestHandler<CloseCashRegisterCommand>
{
    private readonly ICashRegisterRepository _cashRegisters;
    private readonly IUnitOfWork _uow;

    public CloseCashRegisterCommandHandler(ICashRegisterRepository cashRegisters, IUnitOfWork uow)
    {
        _cashRegisters = cashRegisters;
        _uow = uow;
    }

    public async Task Handle(CloseCashRegisterCommand request, CancellationToken cancellationToken)
    {
        var cashRegister = await _cashRegisters.GetOpenByTenantAsync(request.TenantId, cancellationToken)
            ?? throw new NotFoundException("CashRegister", request.TenantId);

        var methodBalances = request.MethodBalances
            .Select(m => CashMethodBalance.Create(m.Method, m.ExpectedAmount, m.CountedAmount))
            .ToList();

        cashRegister.Close(methodBalances, request.EmployeeId, request.Notes);
        await _cashRegisters.UpdateAsync(cashRegister, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
