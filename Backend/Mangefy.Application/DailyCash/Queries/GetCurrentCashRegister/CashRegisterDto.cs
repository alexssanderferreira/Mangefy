using Mangefy.Domain.DailyCash;

namespace Mangefy.Application.DailyCash.Queries.GetCurrentCashRegister;

public sealed record WithdrawalDto(Guid Id, decimal Amount, string Reason, Guid EmployeeId, DateTime CreatedAt);

public sealed record CashRegisterDto(
    Guid Id,
    decimal OpeningAmount,
    decimal? ClosingAmount,
    decimal? ExpectedAmount,
    decimal? Difference,
    string Status,
    Guid OpenedByEmployeeId,
    Guid? ClosedByEmployeeId,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    string? ClosingNotes,
    IReadOnlyList<WithdrawalDto> Withdrawals
)
{
    public static CashRegisterDto FromDomain(CashRegister r) => new(
        r.Id,
        r.OpeningAmount.Amount,
        r.ClosingAmount?.Amount,
        r.ExpectedAmount?.Amount,
        r.Difference,
        r.Status.ToString(),
        r.OpenedByEmployeeId,
        r.ClosedByEmployeeId,
        r.OpenedAt,
        r.ClosedAt,
        r.ClosingNotes,
        r.Withdrawals.Select(w => new WithdrawalDto(w.Id, w.Amount.Amount, w.Reason, w.EmployeeId, w.CreatedAt)).ToList());
}
