using MediatR;

namespace Mangefy.Application.Platform.BusinessTypes.Commands.UpdateRoleTemplate;

public sealed record UpdateRoleTemplateCommand(
    Guid BusinessTypeId,
    Guid TemplateId,
    string Name,
    string? Description,
    IReadOnlyList<string> Permissions) : IRequest;
