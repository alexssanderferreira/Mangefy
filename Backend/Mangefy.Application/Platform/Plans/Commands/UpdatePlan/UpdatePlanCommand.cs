using MediatR;

namespace Mangefy.Application.Platform.Plans.Commands.UpdatePlan;

public sealed record UpdatePlanCommand(
    Guid PlanId,
    decimal MonthlyPrice,
    int MaxTables,
    int MaxMenuItems,
    int MaxUsers,
    int MaxCustomRoles,
    string? Description) : IRequest;
