using MediatR;

namespace Mangefy.Application.Owners.Commands.ResendActivation;

public sealed record ResendActivationCommand(Guid OwnerId) : IRequest<ResendActivationResult>;

public sealed record ResendActivationResult(string ActivationToken, DateTime ExpiresAt, bool EmailSent);
