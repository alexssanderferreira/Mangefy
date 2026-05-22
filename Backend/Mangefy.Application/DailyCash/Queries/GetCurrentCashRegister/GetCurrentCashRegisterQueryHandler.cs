using Mangefy.Domain.DailyCash.Repositories;
using MediatR;

namespace Mangefy.Application.DailyCash.Queries.GetCurrentCashRegister;

public sealed class GetCurrentCashRegisterQueryHandler
    : IRequestHandler<GetCurrentCashRegisterQuery, CashRegisterDto?>
{
    private readonly ICashRegisterRepository _cashRegisters;

    public GetCurrentCashRegisterQueryHandler(ICashRegisterRepository cashRegisters)
        => _cashRegisters = cashRegisters;

    public async Task<CashRegisterDto?> Handle(
        GetCurrentCashRegisterQuery request, CancellationToken cancellationToken)
    {
        var register = await _cashRegisters.GetOpenByTenantAsync(request.TenantId, cancellationToken);
        return register is null ? null : CashRegisterDto.FromDomain(register);
    }
}
