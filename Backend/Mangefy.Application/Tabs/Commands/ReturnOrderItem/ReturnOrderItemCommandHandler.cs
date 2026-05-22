using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Audit;
using Mangefy.Domain.Tabs;
using Mangefy.Domain.Tabs.Repositories;
using MediatR;

namespace Mangefy.Application.Tabs.Commands.ReturnOrderItem;

public sealed class ReturnOrderItemCommandHandler : IRequestHandler<ReturnOrderItemCommand>
{
    private readonly ITabRepository _tabs;
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _audit;
    private readonly ICurrentUser _currentUser;

    public ReturnOrderItemCommandHandler(
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

    public async Task Handle(ReturnOrderItemCommand request, CancellationToken cancellationToken)
    {
        var tab = await _tabs.GetByIdAsync(request.TabId, cancellationToken)
            ?? throw new NotFoundException(nameof(Tab), request.TabId);

        if (tab.TenantId != request.TenantId)
            throw new ForbiddenException();

        tab.ReturnItem(request.OrderId, request.ItemId);

        await _tabs.UpdateAsync(tab, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(
            request.TenantId,
            _currentUser.EmployeeId,
            _currentUser.IsAdminSaas,
            AuditAction.OrderItemReturned,
            nameof(Tab),
            request.TabId,
            ct: cancellationToken);
    }
}
