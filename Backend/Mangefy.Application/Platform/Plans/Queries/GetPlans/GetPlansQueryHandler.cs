using Mangefy.Domain.Plans.Repositories;
using MediatR;

namespace Mangefy.Application.Platform.Plans.Queries.GetPlans;

public sealed class GetPlansQueryHandler : IRequestHandler<GetPlansQuery, IReadOnlyList<PlanDto>>
{
    private readonly IPlanRepository _plans;

    public GetPlansQueryHandler(IPlanRepository plans) => _plans = plans;

    public async Task<IReadOnlyList<PlanDto>> Handle(GetPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await _plans.GetAllAsync(cancellationToken);
        return plans.Select(p => new PlanDto(
            p.Id, p.Name, p.Description,
            p.MonthlyPrice.Amount,
            p.MaxTables, p.MaxMenuItems, p.MaxUsers, p.MaxCustomRoles,
            p.Status.ToString())).ToList();
    }
}
