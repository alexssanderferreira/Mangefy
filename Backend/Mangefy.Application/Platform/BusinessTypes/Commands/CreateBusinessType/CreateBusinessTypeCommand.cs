using MediatR;

namespace Mangefy.Application.Platform.BusinessTypes.Commands.CreateBusinessType;

public sealed record CreateBusinessTypeCommand(string Name, string? Description = null) : IRequest<Guid>;
