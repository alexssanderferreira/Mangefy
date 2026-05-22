using Mangefy.Domain.BusinessSchedules.Repositories;
using MediatR;

namespace Mangefy.Application.Settings.BusinessSchedule.Queries.GetBusinessSchedule;

public sealed class GetBusinessScheduleQueryHandler
    : IRequestHandler<GetBusinessScheduleQuery, BusinessScheduleDto?>
{
    private readonly IBusinessScheduleRepository _repository;

    public GetBusinessScheduleQueryHandler(IBusinessScheduleRepository repository)
        => _repository = repository;

    public async Task<BusinessScheduleDto?> Handle(
        GetBusinessScheduleQuery request, CancellationToken cancellationToken)
    {
        var schedule = await _repository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (schedule is null) return null;

        var weekly = schedule.WeeklySchedule.Select(d => new DayScheduleDto(
            d.DayOfWeek.ToString(),
            !d.IsOpen,
            d.OpenTime?.ToString("HH:mm"),
            d.CloseTime?.ToString("HH:mm"))).ToList();

        var specials = schedule.SpecialDays.Select(s => new SpecialDayDto(
            s.Date.ToString("yyyy-MM-dd"),
            s.IsClosed,
            s.OpenTime?.ToString("HH:mm"),
            s.CloseTime?.ToString("HH:mm"),
            s.Reason)).ToList();

        var policy = new ClosingPolicyDto(
            schedule.ClosingPolicy.GracePeriodMinutes,
            schedule.ClosingPolicy.AllowFinishOpenTabs,
            schedule.ClosingPolicy.BlockedActions.ToList());

        return new BusinessScheduleDto(schedule.Id, weekly, specials, policy);
    }
}
