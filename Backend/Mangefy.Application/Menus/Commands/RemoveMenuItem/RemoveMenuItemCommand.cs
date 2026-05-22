using MediatR;

namespace Mangefy.Application.Menus.Commands.RemoveMenuItem;

public sealed record RemoveMenuItemCommand(Guid TenantId, Guid MenuId, Guid CategoryId, Guid ItemId) : IRequest;
