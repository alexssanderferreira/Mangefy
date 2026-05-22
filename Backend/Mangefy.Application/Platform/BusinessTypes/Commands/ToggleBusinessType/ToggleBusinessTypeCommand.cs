using MediatR;

namespace Mangefy.Application.Platform.BusinessTypes.Commands.ToggleBusinessType;

public sealed record ToggleBusinessTypeCommand(Guid BusinessTypeId, bool Activate) : IRequest;
