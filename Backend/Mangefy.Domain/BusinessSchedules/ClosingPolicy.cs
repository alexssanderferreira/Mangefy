using Mangefy.Domain.Common;

namespace Mangefy.Domain.BusinessSchedules;

/// <summary>
/// Política de fechamento configurada pelo Owner.
/// Define o que acontece com as operações quando o horário de funcionamento termina.
/// </summary>
public sealed class ClosingPolicy : ValueObject
{
    /// <summary>
    /// Minutos de tolerância após o fechamento para finalizar operações em andamento.
    /// Ex: 30 = funcionários têm até 30 min após o fechamento para finalizar.
    /// </summary>
    public int GracePeriodMinutes { get; }

    /// <summary>
    /// Se true, permite fechar (cobrar) comandas abertas durante o período de tolerância.
    /// Se false, bloqueia qualquer movimentação imediatamente ao fechar.
    /// </summary>
    public bool AllowFinishOpenTabs { get; }

    /// <summary>
    /// Permission keys que ficam bloqueadas durante o período de tolerância.
    /// Ex: ["tabs.create", "orders.create"] bloqueia novos pedidos mas permite finalizar existentes.
    /// </summary>
    private readonly List<string> _blockedActions;
    public IReadOnlyCollection<string> BlockedActions => _blockedActions.AsReadOnly();

    private ClosingPolicy(int gracePeriodMinutes, bool allowFinishOpenTabs, List<string> blockedActions)
    {
        GracePeriodMinutes = gracePeriodMinutes;
        AllowFinishOpenTabs = allowFinishOpenTabs;
        _blockedActions = blockedActions;
    }

    public static ClosingPolicy Create(
        int gracePeriodMinutes,
        bool allowFinishOpenTabs,
        IEnumerable<string>? blockedActions = null)
    {
        if (gracePeriodMinutes < 0)
            throw new DomainException("Período de tolerância não pode ser negativo.");

        return new ClosingPolicy(
            gracePeriodMinutes,
            allowFinishOpenTabs,
            blockedActions?.ToList() ?? []);
    }

    public static ClosingPolicy Default() =>
        Create(gracePeriodMinutes: 30, allowFinishOpenTabs: true,
               blockedActions: ["tabs.create", "orders.create"]);

    public bool IsActionBlocked(string permissionKey) => _blockedActions.Contains(permissionKey);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return GracePeriodMinutes;
        yield return AllowFinishOpenTabs;
        foreach (var a in _blockedActions.OrderBy(x => x))
            yield return a;
    }
}
