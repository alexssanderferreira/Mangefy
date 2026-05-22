using MediatR;

namespace Mangefy.Application.Owners.Commands.ActivateOwner;

public sealed record ActivateOwnerCommand(Guid OwnerId) : IRequest;
