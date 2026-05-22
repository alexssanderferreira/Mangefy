using MediatR;

namespace Mangefy.Application.Platform.Plans.Queries.GetPlans;

public sealed record GetPlansQuery : IRequest<IReadOnlyList<PlanDto>>;

public sealed record PlanDto(
    Guid Id,
    string Name,
    string? Description,
    decimal MonthlyPrice,
    int MaxTables,
    int MaxMenuItems,
    int MaxUsers,
    int MaxCustomRoles,
    string Status);
