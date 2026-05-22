using MediatR;

namespace Mangefy.Application.Menus.Commands.SetMenuItemStatus;

public sealed record SetMenuItemStatusCommand(
    Guid TenantId,
    Guid MenuId,
    Guid CategoryId,
    Guid ItemId,
    string Status) : IRequest;
