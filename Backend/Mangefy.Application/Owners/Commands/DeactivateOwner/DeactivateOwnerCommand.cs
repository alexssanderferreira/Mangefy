using MediatR;

namespace Mangefy.Application.Owners.Commands.DeactivateOwner;

public sealed record DeactivateOwnerCommand(Guid OwnerId) : IRequest;
