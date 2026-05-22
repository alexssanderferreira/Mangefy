using Mangefy.Domain.Common;

namespace Mangefy.Domain.BusinessSchedules;

/// <summary>
/// Data especial que sobrepõe o horário semanal padrão.
/// Pode ser um feriado (fechado) ou uma data com horário diferente do habitual.
/// </summary>
public sealed class SpecialDay : Entity
{
    public DateOnly Date { get; private set; }
    public bool IsClosed { get; private set; }
    public TimeOnly? OpenTime { get; private set; }
    public TimeOnly? CloseTime { get; private set; }
    public string Reason { get; private set; }

    private SpecialDay() { }

    internal static SpecialDay CreateHoliday(DateOnly date, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Motivo do feriado não pode ser vazio.");

        return new SpecialDay
        {
            Date = date,
            IsClosed = true,
            OpenTime = null,
            CloseTime = null,
            Reason = reason.Trim()
        };
    }

    internal static SpecialDay CreateWithCustomHours(
        DateOnly date, TimeOnly openTime, TimeOnly closeTime, string reason)
    {
        if (openTime >= closeTime)
            throw new DomainException("Horário de abertura deve ser anterior ao de fechamento.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Motivo do dia especial não pode ser vazio.");

        return new SpecialDay
        {
            Date = date,
            IsClosed = false,
            OpenTime = openTime,
            CloseTime = closeTime,
            Reason = reason.Trim()
        };
    }

    internal void Update(bool isClosed, TimeOnly? openTime, TimeOnly? closeTime, string reason)
    {
        if (!isClosed && (openTime is null || closeTime is null))
            throw new DomainException("Horário de abertura e fechamento são obrigatórios para dia aberto.");

        if (!isClosed && openTime >= closeTime)
            throw new DomainException("Horário de abertura deve ser anterior ao de fechamento.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Motivo não pode ser vazio.");

        IsClosed = isClosed;
        OpenTime = isClosed ? null : openTime;
        CloseTime = isClosed ? null : closeTime;
        Reason = reason.Trim();
        SetUpdatedAt();
    }
}
