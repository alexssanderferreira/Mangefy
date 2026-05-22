using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Roles.Repositories;
using MediatR;

namespace Mangefy.Application.Roles.Commands.UpdateRolePermissions;

public sealed class UpdateRolePermissionsCommandHandler : IRequestHandler<UpdateRolePermissionsCommand>
{
    private readonly ITenantRoleRepository _roles;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _uow;

    public UpdateRolePermissionsCommandHandler(
        ITenantRoleRepository roles,
        ICurrentUser currentUser,
        IUnitOfWork uow)
    {
        _roles = roles;
        _currentUser = currentUser;
        _uow = uow;
    }

    public async Task Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = await _roles.GetByIdAsync(request.RoleId, cancellationToken)
            ?? throw new NotFoundException("TenantRole", request.RoleId);

        if (role.TenantId != request.TenantId)
            throw new ForbiddenException();

        if (role.IsOwnerRole)
            throw new ForbiddenException("Não é possível alterar permissões do cargo Owner.");

        // Impedir escalada de privilégio: usuário sem override não pode atribuir permissões que não possui
        if (!_currentUser.IsAdminSaas)
        {
            var illegalPerms = request.Permissions
                .Where(p => !_currentUser.HasPermission(p))
                .ToList();

            if (illegalPerms.Count > 0)
                throw new ForbiddenException(
                    $"Você não pode atribuir permissões que não possui: {string.Join(", ", illegalPerms)}");
        }

        role.SetPermissions(request.Permissions);
        await _roles.UpdateAsync(role, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
