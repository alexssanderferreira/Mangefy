using MediatR;

namespace Mangefy.Application.Platform.BusinessTypes.Commands.DeleteBusinessType;

public sealed record DeleteBusinessTypeCommand(Guid BusinessTypeId) : IRequest;
