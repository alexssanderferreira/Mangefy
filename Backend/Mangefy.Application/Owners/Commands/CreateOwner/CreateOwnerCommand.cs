using MediatR;

namespace Mangefy.Application.Owners.Commands.CreateOwner;

public sealed record CreateOwnerCommand(string Name, string Email) : IRequest<CreateOwnerResult>;

public sealed record CreateOwnerResult(Guid Id, string ActivationToken, DateTime ActivationTokenExpiresAt);
