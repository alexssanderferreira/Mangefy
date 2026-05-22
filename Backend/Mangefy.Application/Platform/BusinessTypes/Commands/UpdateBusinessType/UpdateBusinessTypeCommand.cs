using MediatR;

namespace Mangefy.Application.Platform.BusinessTypes.Commands.UpdateBusinessType;

public sealed record UpdateBusinessTypeCommand(Guid BusinessTypeId, string Name, string? Description) : IRequest;
