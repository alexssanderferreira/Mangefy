using MediatR;

namespace Mangefy.Application.Platform.BusinessTypes.Commands.DeleteRoleTemplate;

public sealed record DeleteRoleTemplateCommand(Guid BusinessTypeId, Guid TemplateId) : IRequest;
