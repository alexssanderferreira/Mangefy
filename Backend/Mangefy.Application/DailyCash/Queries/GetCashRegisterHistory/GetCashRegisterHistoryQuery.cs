using Mangefy.Application.DailyCash.Queries.GetCurrentCashRegister;
using MediatR;

namespace Mangefy.Application.DailyCash.Queries.GetCashRegisterHistory;

public sealed record GetCashRegisterHistoryQuery(Guid TenantId, DateOnly From, DateOnly To)
    : IRequest<IReadOnlyList<CashRegisterDto>>;
