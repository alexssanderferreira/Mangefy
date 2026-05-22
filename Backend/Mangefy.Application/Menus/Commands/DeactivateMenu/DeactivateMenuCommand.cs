using MediatR;

namespace Mangefy.Application.Menus.Commands.DeactivateMenu;

public sealed record DeactivateMenuCommand(Guid TenantId, Guid MenuId) : IRequest;
