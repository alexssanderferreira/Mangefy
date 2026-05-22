using MediatR;

namespace Mangefy.Application.Platform.Plans.Commands.DeletePlan;

public sealed record DeletePlanCommand(Guid PlanId) : IRequest;
