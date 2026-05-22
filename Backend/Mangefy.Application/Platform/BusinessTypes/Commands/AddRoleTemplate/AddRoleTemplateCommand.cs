using MediatR;

namespace Mangefy.Application.Platform.BusinessTypes.Commands.AddRoleTemplate;

public sealed record AddRoleTemplateCommand(
    Guid BusinessTypeId,
    string Name,
    string? Description,
    IReadOnlyList<string> Permissions) : IRequest<Guid>;
