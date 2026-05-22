using MediatR;

namespace Mangefy.Application.Platform.Plans.Commands.ActivatePlan;

public sealed record ActivatePlanCommand(Guid PlanId) : IRequest;
