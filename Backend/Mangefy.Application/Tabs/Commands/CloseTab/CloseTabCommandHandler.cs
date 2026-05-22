using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Audit;
using Mangefy.Domain.Common;
using Mangefy.Domain.Fiscal;
using Mangefy.Domain.Fiscal.Repositories;
using Mangefy.Domain.Roles;
using Mangefy.Domain.Settings;
using Mangefy.Domain.Settings.Repositories;
using Mangefy.Domain.Tabs;
using Mangefy.Domain.Tabs.Repositories;
using MediatR;

namespace Mangefy.Application.Tabs.Commands.CloseTab;

public sealed class CloseTabCommandHandler : IRequestHandler<CloseTabCommand>
{
    private readonly ITabRepository _tabs;
    private readonly IFiscalSettingsRepository _fiscalSettings;
    private readonly IFiscalDocumentRepository _fiscalDocs;
    private readonly ITabSettingsRepository _tabSettings;
    private readonly IPaymentSettingsRepository _paymentSettings;
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _audit;
    private readonly ICurrentUser _currentUser;

    public CloseTabCommandHandler(
        ITabRepository tabs,
        IFiscalSettingsRepository fiscalSettings,
        IFiscalDocumentRepository fiscalDocs,
        ITabSettingsRepository tabSettings,
        IPaymentSettingsRepository paymentSettings,
        IUnitOfWork uow,
        IAuditService audit,
        ICurrentUser currentUser)
    {
        _tabs = tabs;
        _fiscalSettings = fiscalSettings;
        _fiscalDocs = fiscalDocs;
        _tabSettings = tabSettings;
        _paymentSettings = paymentSettings;
        _uow = uow;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(CloseTabCommand request, CancellationToken cancellationToken)
    {
        var tab = await _tabs.GetByIdAsync(request.TabId, cancellationToken)
            ?? throw new NotFoundException(nameof(Tab), request.TabId);

        if (tab.TenantId != request.TenantId)
            throw new ForbiddenException();

        // Validar métodos de pagamento habilitados
        var paySettings = await _paymentSettings.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (paySettings is not null)
        {
            foreach (var p in request.Payments)
            {
                if (!paySettings.IsMethodEnabled(p.Method))
                    throw new DomainException(
                        $"Método de pagamento '{p.Method}' não está habilitado para este estabelecimento.");
            }
        }

        if (request.DiscountAmount > 0)
        {
            await EnforceDiscountPolicyAsync(request.TenantId, request.DiscountAmount, tab.Subtotal.Amount, request.DiscountReason, cancellationToken);
            tab.ApplyTabDiscount(request.DiscountAmount);
        }

        if (request.ServiceFee > 0)
            tab.ApplyServiceFee(request.ServiceFee);

        if (request.Tip > 0)
            tab.ApplyTip(request.Tip);

        foreach (var p in request.Payments)
            tab.AddPayment(p.Amount, p.Method, p.ChangeGiven, p.ExternalReference);

        tab.Close();

        await _tabs.UpdateAsync(tab, cancellationToken);

        // Documento fiscal preparatório
        var fiscal = await _fiscalSettings.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (fiscal is { NfceEnabled: true, AutoEmitOnTabClose: true })
        {
            var doc = FiscalDocument.CreatePending(
                request.TenantId,
                tab.Id,
                FiscalDocumentType.NfcE,
                FiscalEnvironment.Producao,
                tab.Total.Amount);

            await _fiscalDocs.AddAsync(doc, cancellationToken);
        }

        await _uow.SaveChangesAsync(cancellationToken);

        if (request.DiscountAmount > 0)
            await _audit.LogAsync(
                request.TenantId,
                _currentUser.EmployeeId,
                _currentUser.IsAdminSaas,
                AuditAction.TabDiscountApplied,
                nameof(Tab),
                tab.Id,
                after: $"Desconto: R$ {request.DiscountAmount:F2}",
                ct: cancellationToken);
    }

    private async Task EnforceDiscountPolicyAsync(
        Guid tenantId,
        decimal discountAmount,
        decimal subtotal,
        string? discountReason,
        CancellationToken ct)
    {
        // Owner ou quem tem override pode descontar qualquer valor
        if (_currentUser.HasPermission(PermissionCatalog.Tabs.ApplyDiscountOverride))
            return;

        if (!_currentUser.HasPermission(PermissionCatalog.Tabs.ApplyDiscount))
            throw new ForbiddenException(PermissionCatalog.Tabs.ApplyDiscount);

        var settings = await _tabSettings.GetByTenantIdAsync(tenantId, ct);
        if (settings is null) return;

        if (subtotal > 0)
        {
            var discountPercent = discountAmount / subtotal * 100m;
            if (discountPercent > settings.MaxDiscountPercent)
                throw new ForbiddenException(
                    $"{PermissionCatalog.Tabs.ApplyDiscountOverride} (desconto de {discountPercent:F1}% excede limite de {settings.MaxDiscountPercent:F1}%)");
        }

        // Verificar se motivo é obrigatório acima do limite
        if (settings.DiscountReasonRequiredAbove.HasValue
            && discountAmount > settings.DiscountReasonRequiredAbove.Value
            && string.IsNullOrWhiteSpace(discountReason))
        {
            throw new DomainException(
                $"Motivo de desconto é obrigatório para descontos acima de R$ {settings.DiscountReasonRequiredAbove.Value:F2}.");
        }
    }
}
