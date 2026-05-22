using Mangefy.Application.DailyCash.Queries.GetCurrentCashRegister;
using Mangefy.Domain.DailyCash.Repositories;
using MediatR;

namespace Mangefy.Application.DailyCash.Queries.GetCashRegisterHistory;

public sealed class GetCashRegisterHistoryQueryHandler
    : IRequestHandler<GetCashRegisterHistoryQuery, IReadOnlyList<CashRegisterDto>>
{
    private readonly ICashRegisterRepository _cashRegisters;

    public GetCashRegisterHistoryQueryHandler(ICashRegisterRepository cashRegisters)
        => _cashRegisters = cashRegisters;

    public async Task<IReadOnlyList<CashRegisterDto>> Handle(
        GetCashRegisterHistoryQuery request, CancellationToken cancellationToken)
    {
        var registers = await _cashRegisters.GetHistoryByTenantAsync(
            request.TenantId, request.From, request.To, cancellationToken);
        return registers.Select(CashRegisterDto.FromDomain).ToList();
    }
}
