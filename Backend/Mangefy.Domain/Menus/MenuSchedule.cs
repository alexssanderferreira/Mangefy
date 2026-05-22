using Mangefy.Domain.Common;

namespace Mangefy.Domain.Menus;

/// <summary>
/// Vigência automática de um cardápio por dias da semana e faixa de horário.
/// Opcional — se nulo, o cardápio só é exibido via ativação manual.
/// </summary>
public sealed class MenuSchedule : ValueObject
{
    private readonly List<DayOfWeek> _activeDays;
    public IReadOnlyList<DayOfWeek> ActiveDays => _activeDays.AsReadOnly();
    public TimeOnly StartTime { get; }
    public TimeOnly EndTime { get; }

    private MenuSchedule(List<DayOfWeek> activeDays, TimeOnly startTime, TimeOnly endTime)
    {
        _activeDays = activeDays;
        StartTime = startTime;
        EndTime = endTime;
    }

    public static MenuSchedule Create(IEnumerable<DayOfWeek> activeDays, TimeOnly startTime, TimeOnly endTime)
    {
        var days = activeDays.Distinct().ToList();

        if (days.Count == 0)
            throw new DomainException("O cardápio deve estar ativo em pelo menos um dia da semana.");

        if (startTime >= endTime)
            throw new DomainException("Horário de início deve ser anterior ao de fim.");

        return new MenuSchedule(days, startTime, endTime);
    }

    public bool IsActiveAt(DayOfWeek day, TimeOnly time) =>
        _activeDays.Contains(day) && time >= StartTime && time <= EndTime;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        foreach (var d in _activeDays.OrderBy(x => x)) yield return d;
        yield return StartTime;
        yield return EndTime;
    }
}
