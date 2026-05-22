using MediatR;

namespace Mangefy.Application.Menus.Commands.RemoveMenuCategory;

public sealed record RemoveMenuCategoryCommand(Guid TenantId, Guid MenuId, Guid CategoryId) : IRequest;
