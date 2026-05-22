using Mangefy.Domain.Common;

namespace Mangefy.Domain.BusinessSchedules;

public sealed class BusinessSchedule : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public ClosingPolicy ClosingPolicy { get; private set; }

    private readonly List<DaySchedule> _weeklySchedule = [];
    private readonly List<SpecialDay> _specialDays = [];

    public IReadOnlyList<DaySchedule> WeeklySchedule => _weeklySchedule.AsReadOnly();
    public IReadOnlyList<SpecialDay> SpecialDays => _specialDays.AsReadOnly();

    private BusinessSchedule() { }

    public static BusinessSchedule Create(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        var schedule = new BusinessSchedule
        {
            TenantId = tenantId,
            ClosingPolicy = ClosingPolicy.Default()
        };

        // Initialize all 7 days as closed by default
        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
            schedule._weeklySchedule.Add(DaySchedule.Closed(day));

        return schedule;
    }

    public void SetDaySchedule(DayOfWeek day, TimeOnly openTime, TimeOnly closeTime)
    {
        _weeklySchedule.RemoveAll(d => d.DayOfWeek == day);
        _weeklySchedule.Add(DaySchedule.Open(day, openTime, closeTime));
        SetUpdatedAt();
    }

    public void CloseDayOfWeek(DayOfWeek day)
    {
        _weeklySchedule.RemoveAll(d => d.DayOfWeek == day);
        _weeklySchedule.Add(DaySchedule.Closed(day));
        SetUpdatedAt();
    }

    public void AddHoliday(DateOnly date, string reason)
    {
        RemoveSpecialDay(date);
        _specialDays.Add(SpecialDay.CreateHoliday(date, reason));
        SetUpdatedAt();
    }

    public void AddSpecialDayWithCustomHours(
        DateOnly date, TimeOnly openTime, TimeOnly closeTime, string reason)
    {
        RemoveSpecialDay(date);
        _specialDays.Add(SpecialDay.CreateWithCustomHours(date, openTime, closeTime, reason));
        SetUpdatedAt();
    }

    public void UpdateSpecialDay(
        DateOnly date, bool isClosed, TimeOnly? openTime, TimeOnly? closeTime, string reason)
    {
        var specialDay = _specialDays.FirstOrDefault(d => d.Date == date)
            ?? throw new DomainException($"Dia especial {date} não encontrado.");

        specialDay.Update(isClosed, openTime, closeTime, reason);
        SetUpdatedAt();
    }

    public void RemoveSpecialDay(DateOnly date)
    {
        var existing = _specialDays.FirstOrDefault(d => d.Date == date);
        if (existing is not null)
        {
            _specialDays.Remove(existing);
            SetUpdatedAt();
        }
    }

    public void UpdateClosingPolicy(int gracePeriodMinutes, bool allowFinishOpenTabs, IEnumerable<string>? blockedActions = null)
    {
        ClosingPolicy = ClosingPolicy.Create(gracePeriodMinutes, allowFinishOpenTabs, blockedActions);
        SetUpdatedAt();
    }

    /// <summary>
    /// Verifica se o estabelecimento está aberto em uma data e hora específicas.
    /// Dias especiais têm prioridade sobre a grade semanal.
    /// </summary>
    public bool IsOpenAt(DateOnly date, TimeOnly time)
    {
        var specialDay = _specialDays.FirstOrDefault(d => d.Date == date);
        if (specialDay is not null)
        {
            if (specialDay.IsClosed) return false;
            if (specialDay.OpenTime is null || specialDay.CloseTime is null) return false;
            return time >= specialDay.OpenTime.Value && time <= specialDay.CloseTime.Value;
        }

        var daySchedule = _weeklySchedule.FirstOrDefault(d => d.DayOfWeek == date.DayOfWeek);
        return daySchedule?.IsWithinOperatingHours(time) ?? false;
    }

    /// <summary>
    /// Verifica se o estabelecimento está dentro do período de tolerância (ainda operando).
    /// </summary>
    public bool IsWithinGracePeriod(DateOnly date, TimeOnly time)
    {
        var specialDay = _specialDays.FirstOrDefault(d => d.Date == date);
        if (specialDay is not null)
        {
            if (specialDay.IsClosed) return false;
            if (specialDay.OpenTime is null || specialDay.CloseTime is null) return false;
            var closeWithGrace = specialDay.CloseTime.Value.AddMinutes(ClosingPolicy.GracePeriodMinutes);
            return time >= specialDay.OpenTime.Value && time <= closeWithGrace;
        }

        var daySchedule = _weeklySchedule.FirstOrDefault(d => d.DayOfWeek == date.DayOfWeek);
        return daySchedule?.IsWithinOperatingHours(time, ClosingPolicy.GracePeriodMinutes) ?? false;
    }

    public DaySchedule? GetDaySchedule(DayOfWeek day) =>
        _weeklySchedule.FirstOrDefault(d => d.DayOfWeek == day);
}
