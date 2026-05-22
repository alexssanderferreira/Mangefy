using Mangefy.Domain.Common;
using Mangefy.Domain.Common.ValueObjects;
using Mangefy.Domain.DailyCash.Events;

namespace Mangefy.Domain.DailyCash;

public sealed class CashRegister : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Money OpeningAmount { get; private set; } = null!;
    public Money? ClosingAmount { get; private set; }
    public Money? ExpectedAmount { get; private set; }
    public CashRegisterStatus Status { get; private set; }
    public Guid OpenedByEmployeeId { get; private set; }
    public Guid? ClosedByEmployeeId { get; private set; }
    public DateTime OpenedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public string? ClosingNotes { get; private set; }
    public uint RowVersion { get; private set; }

    private readonly List<CashWithdrawal> _withdrawals = [];
    private readonly List<CashSupply> _supplies = [];
    private readonly List<CashMethodBalance> _methodBalances = [];

    public IReadOnlyList<CashWithdrawal> Withdrawals => _withdrawals.AsReadOnly();
    public IReadOnlyList<CashSupply> Supplies => _supplies.AsReadOnly();
    public IReadOnlyList<CashMethodBalance> MethodBalances => _methodBalances.AsReadOnly();

    private CashRegister() { }

    public static CashRegister Open(Guid tenantId, decimal openingAmount, Guid employeeId)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        if (openingAmount < 0)
            throw new DomainException("Valor de abertura não pode ser negativo.");

        var register = new CashRegister
        {
            TenantId = tenantId,
            OpeningAmount = Money.Create(openingAmount),
            Status = CashRegisterStatus.Open,
            OpenedByEmployeeId = employeeId,
            OpenedAt = DateTime.UtcNow
        };

        register.AddDomainEvent(new CashRegisterOpenedEvent(register.Id, tenantId, employeeId));
        return register;
    }

    public void RegisterWithdrawal(decimal amount, string reason, Guid employeeId)
    {
        EnsureOpen();
        _withdrawals.Add(CashWithdrawal.Create(amount, reason, employeeId));
        SetUpdatedAt();
    }

    public void RegisterSupply(decimal amount, string reason, Guid employeeId)
    {
        EnsureOpen();
        _supplies.Add(CashSupply.Create(amount, reason, employeeId));
        SetUpdatedAt();
    }

    /// <summary>
    /// Fecha o caixa com balanço detalhado por método de pagamento.
    /// Requer observação quando há divergência em qualquer método.
    /// </summary>
    public void Close(
        IReadOnlyList<CashMethodBalance> methodBalances,
        Guid employeeId,
        string? notes = null)
    {
        EnsureOpen();

        if (methodBalances is null || methodBalances.Count == 0)
            throw new DomainException("Informe o balanço por método de pagamento para fechar o caixa.");

        var hasDivergence = methodBalances.Any(b => Math.Abs(b.Difference) > 0.01m);

        if (hasDivergence && string.IsNullOrWhiteSpace(notes))
            throw new DomainException(
                "Observação obrigatória quando há divergência de valor em algum método de pagamento.");

        _methodBalances.Clear();
        _methodBalances.AddRange(methodBalances);

        var totalCounted = methodBalances.Sum(b => b.CountedAmount);
        var totalExpected = methodBalances.Sum(b => b.ExpectedAmount);

        ClosingAmount = Money.Create(totalCounted);
        ExpectedAmount = Money.Create(totalExpected);
        Status = CashRegisterStatus.Closed;
        ClosedByEmployeeId = employeeId;
        ClosedAt = DateTime.UtcNow;
        ClosingNotes = notes?.Trim();

        SetUpdatedAt();
        AddDomainEvent(new CashRegisterClosedEvent(Id, TenantId, totalExpected, totalCounted, employeeId));
    }

    public Money TotalWithdrawals =>
        Money.Create(_withdrawals.Sum(w => w.Amount.Amount));

    public Money TotalSupplies =>
        Money.Create(_supplies.Sum(s => s.Amount.Amount));

    /// <summary>
    /// Diferença entre o valor contado e o esperado. Positivo = sobra, negativo = falta.
    /// Usa decimal diretamente pois Money.Create rejeita negativos.
    /// </summary>
    public decimal? Difference =>
        ClosingAmount is null || ExpectedAmount is null
            ? null
            : Math.Round(ClosingAmount.Amount - ExpectedAmount.Amount, 2);

    private void EnsureOpen()
    {
        if (Status != CashRegisterStatus.Open)
            throw new DomainException("O caixa não está aberto.");
    }
}
