using MediatR;

namespace Mangefy.Application.Settings.BusinessSchedule.Commands.UpdateBusinessSchedule;

public sealed record DayScheduleInput(string DayOfWeek, bool IsClosed, string? OpenTime, string? CloseTime);

public sealed record SpecialDayInput(string Date, bool IsClosed, string? OpenTime, string? CloseTime, string Reason);

public sealed record ClosingPolicyInput(int GracePeriodMinutes, bool AllowFinishOpenTabs, IReadOnlyList<string>? BlockedActions);

public sealed record UpdateBusinessScheduleCommand(
    Guid TenantId,
    IReadOnlyList<DayScheduleInput>? WeeklySchedule,
    IReadOnlyList<SpecialDayInput>? SpecialDays,
    ClosingPolicyInput? ClosingPolicy) : IRequest;
