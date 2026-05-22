using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Audit;
using Mangefy.Domain.Common;
using Mangefy.Domain.Roles;
using Mangefy.Domain.Tabs;
using Mangefy.Domain.Tabs.Repositories;
using MediatR;

namespace Mangefy.Application.Tabs.Commands.CancelOrderItem;

public sealed class CancelOrderItemCommandHandler : IRequestHandler<CancelOrderItemCommand>
{
    private readonly ITabRepository _tabs;
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _audit;
    private readonly ICurrentUser _currentUser;

    public CancelOrderItemCommandHandler(
        ITabRepository tabs,
        IUnitOfWork uow,
        IAuditService audit,
        ICurrentUser currentUser)
    {
        _tabs = tabs;
        _uow = uow;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(CancelOrderItemCommand request, CancellationToken cancellationToken)
    {
        var tab = await _tabs.GetByIdAsync(request.TabId, cancellationToken)
            ?? throw new NotFoundException(nameof(Tab), request.TabId);

        if (tab.TenantId != request.TenantId)
            throw new ForbiddenException();

        var order = tab.Orders.FirstOrDefault(o => o.Id == request.OrderId)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        var item = order.Items.FirstOrDefault(i => i.Id == request.ItemId)
            ?? throw new NotFoundException(nameof(OrderItem), request.ItemId);

        EnforcePolicy(item, request.Reason);

        tab.CancelItem(request.OrderId, request.ItemId, request.Reason);

        await _tabs.UpdateAsync(tab, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(
            request.TenantId,
            _currentUser.EmployeeId,
            _currentUser.IsAdminSaas,
            AuditAction.OrderItemCancelled,
            nameof(Tab),
            request.TabId,
            reason: request.Reason,
            ct: cancellationToken);
    }

    private void EnforcePolicy(OrderItem item, string? reason)
    {
        // Pending → qualquer employee com orders.cancel pode remover sem motivo
        if (item.Status == OrderItemStatus.Pending)
            return;

        // Sent ou Preparing → requer permissão específica + motivo
        if (item.Status is OrderItemStatus.Sent or OrderItemStatus.Preparing)
        {
            if (!_currentUser.HasPermission(PermissionCatalog.Orders.CancelAfterSent) &&
                !_currentUser.HasPermission(PermissionCatalog.Orders.CancelInPreparation))
                throw new ForbiddenException(PermissionCatalog.Orders.CancelAfterSent);

            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException(
                    "Motivo obrigatório para cancelar item já enviado para a cozinha.");
        }

        // Ready → requer permissão de preparo + motivo
        if (item.Status == OrderItemStatus.Ready)
        {
            if (!_currentUser.HasPermission(PermissionCatalog.Orders.CancelInPreparation))
                throw new ForbiddenException(PermissionCatalog.Orders.CancelInPreparation);

            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException("Motivo obrigatório para cancelar item pronto.");
        }

        // Delivered → requer permissão gerencial + motivo
        if (item.Status == OrderItemStatus.Delivered)
        {
            if (!_currentUser.HasPermission(PermissionCatalog.Orders.CancelDelivered))
                throw new ForbiddenException(PermissionCatalog.Orders.CancelDelivered);

            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException("Motivo obrigatório para cancelar item já entregue.");
        }
    }
}
