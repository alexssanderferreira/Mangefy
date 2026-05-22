using MediatR;

namespace Mangefy.Application.Platform.Plans.Commands.CreatePlan;

public sealed record CreatePlanCommand(
    string Name,
    decimal MonthlyPrice,
    int MaxTables,
    int MaxMenuItems,
    int MaxUsers,
    int MaxCustomRoles = 0,
    string? Description = null) : IRequest<Guid>;
