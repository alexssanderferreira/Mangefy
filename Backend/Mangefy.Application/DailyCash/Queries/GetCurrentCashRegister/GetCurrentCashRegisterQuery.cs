using MediatR;

namespace Mangefy.Application.DailyCash.Queries.GetCurrentCashRegister;

public sealed record GetCurrentCashRegisterQuery(Guid TenantId)
    : IRequest<CashRegisterDto?>;
