using MediatR;

namespace Mangefy.Application.Platform.BusinessTypes.Commands.ToggleRoleTemplate;

public sealed record ToggleRoleTemplateCommand(Guid BusinessTypeId, Guid TemplateId, bool Activate) : IRequest;
