using Mangefy.Domain.Common;

namespace Mangefy.Domain.EmployeeSchedules;

/// <summary>
/// Turno de trabalho de um funcionário para um dia específico da semana.
/// </summary>
public sealed class DayShift : ValueObject
{
    public DayOfWeek DayOfWeek { get; }
    public bool IsWorkDay { get; }
    public TimeOnly? StartTime { get; }
    public TimeOnly? EndTime { get; }

    private DayShift(DayOfWeek dayOfWeek, bool isWorkDay, TimeOnly? startTime, TimeOnly? endTime)
    {
        DayOfWeek = dayOfWeek;
        IsWorkDay = isWorkDay;
        StartTime = startTime;
        EndTime = endTime;
    }

    public static DayShift Working(DayOfWeek day, TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime >= endTime)
            throw new DomainException(
                $"Horário de início deve ser anterior ao de fim para {day}.");

        return new DayShift(day, true, startTime, endTime);
    }

    public static DayShift DayOff(DayOfWeek day) =>
        new(day, false, null, null);

    public bool IsOnDuty(TimeOnly time) =>
        IsWorkDay && StartTime is not null && EndTime is not null
        && time >= StartTime.Value && time <= EndTime.Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return DayOfWeek;
        yield return IsWorkDay;
        yield return StartTime ?? TimeOnly.MinValue;
        yield return EndTime ?? TimeOnly.MinValue;
    }
}
