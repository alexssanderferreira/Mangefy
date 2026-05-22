using Mangefy.Domain.Common;

namespace Mangefy.Domain.EmployeeSchedules;

public sealed class EmployeeSchedule : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }

    private readonly List<DayShift> _weeklyShifts = [];
    public IReadOnlyList<DayShift> WeeklyShifts => _weeklyShifts.AsReadOnly();

    private EmployeeSchedule() { }

    public static EmployeeSchedule Create(Guid tenantId, Guid employeeId)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        if (employeeId == Guid.Empty)
            throw new DomainException("EmployeeId inválido.");

        var schedule = new EmployeeSchedule
        {
            TenantId = tenantId,
            EmployeeId = employeeId
        };

        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
            schedule._weeklyShifts.Add(DayShift.DayOff(day));

        return schedule;
    }

    public void SetWorkDay(DayOfWeek day, TimeOnly startTime, TimeOnly endTime)
    {
        _weeklyShifts.RemoveAll(s => s.DayOfWeek == day);
        _weeklyShifts.Add(DayShift.Working(day, startTime, endTime));
        SetUpdatedAt();
    }

    public void SetDayOff(DayOfWeek day)
    {
        _weeklyShifts.RemoveAll(s => s.DayOfWeek == day);
        _weeklyShifts.Add(DayShift.DayOff(day));
        SetUpdatedAt();
    }

    public bool IsOnDutyAt(DayOfWeek day, TimeOnly time)
    {
        var shift = _weeklyShifts.FirstOrDefault(s => s.DayOfWeek == day);
        return shift?.IsOnDuty(time) ?? false;
    }

    public DayShift? GetShift(DayOfWeek day) =>
        _weeklyShifts.FirstOrDefault(s => s.DayOfWeek == day);
}
