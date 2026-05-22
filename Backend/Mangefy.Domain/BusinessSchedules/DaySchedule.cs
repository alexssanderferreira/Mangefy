using Mangefy.Domain.Common;

namespace Mangefy.Domain.BusinessSchedules;

/// <summary>
/// Horário de funcionamento de um dia específico da semana.
/// </summary>
public sealed class DaySchedule : ValueObject
{
    public DayOfWeek DayOfWeek { get; }
    public bool IsOpen { get; }
    public TimeOnly? OpenTime { get; }
    public TimeOnly? CloseTime { get; }

    private DaySchedule(DayOfWeek dayOfWeek, bool isOpen, TimeOnly? openTime, TimeOnly? closeTime)
    {
        DayOfWeek = dayOfWeek;
        IsOpen = isOpen;
        OpenTime = openTime;
        CloseTime = closeTime;
    }

    public static DaySchedule Open(DayOfWeek day, TimeOnly openTime, TimeOnly closeTime)
    {
        if (openTime >= closeTime)
            throw new DomainException(
                $"Horário de abertura deve ser anterior ao de fechamento para {day}.");

        return new DaySchedule(day, true, openTime, closeTime);
    }

    public static DaySchedule Closed(DayOfWeek day) =>
        new(day, false, null, null);

    /// <summary>
    /// Verifica se um horário está dentro do expediente, considerando a tolerância de fechamento.
    /// </summary>
    public bool IsWithinOperatingHours(TimeOnly time, int gracePeriodMinutes = 0)
    {
        if (!IsOpen || OpenTime is null || CloseTime is null) return false;

        var closeWithGrace = CloseTime.Value.AddMinutes(gracePeriodMinutes);
        return time >= OpenTime.Value && time <= closeWithGrace;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return DayOfWeek;
        yield return IsOpen;
        yield return OpenTime ?? TimeOnly.MinValue;
        yield return CloseTime ?? TimeOnly.MinValue;
    }
}
