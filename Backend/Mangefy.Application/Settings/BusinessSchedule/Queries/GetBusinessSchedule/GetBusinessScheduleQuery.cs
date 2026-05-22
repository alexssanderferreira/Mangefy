using MediatR;

namespace Mangefy.Application.Settings.BusinessSchedule.Queries.GetBusinessSchedule;

public sealed record DayScheduleDto(string DayOfWeek, bool IsClosed, string? OpenTime, string? CloseTime);

public sealed record SpecialDayDto(string Date, bool IsClosed, string? OpenTime, string? CloseTime, string? Reason);

public sealed record ClosingPolicyDto(int GracePeriodMinutes, bool AllowFinishOpenTabs, IReadOnlyList<string> BlockedActions);

public sealed record BusinessScheduleDto(
    Guid Id,
    IReadOnlyList<DayScheduleDto> WeeklySchedule,
    IReadOnlyList<SpecialDayDto> SpecialDays,
    ClosingPolicyDto ClosingPolicy);

public sealed record GetBusinessScheduleQuery(Guid TenantId)
    : IRequest<BusinessScheduleDto?>;
