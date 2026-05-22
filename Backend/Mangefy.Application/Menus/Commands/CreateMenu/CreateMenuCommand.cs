using MediatR;

namespace Mangefy.Application.Menus.Commands.CreateMenu;

public sealed record CreateMenuCommand(
    Guid TenantId,
    string Name,
    IReadOnlyList<DayOfWeek>? ScheduleDays,
    TimeOnly? ScheduleStart,
    TimeOnly? ScheduleEnd
) : IRequest<Guid>;
