using MediatR;

namespace Mangefy.Application.Menus.Commands.ActivateMenu;

public sealed record ActivateMenuCommand(Guid TenantId, Guid MenuId) : IRequest;
