namespace Mangefy.Domain.Audit;

public static class AuditAction
{
    public const string EmployeeCreated        = "employee.created";
    public const string EmployeeDeactivated    = "employee.deactivated";
    public const string EmployeePasswordSet    = "employee.password_set";
    public const string EmployeeAccessGranted  = "employee.access_granted";

    public const string TenantSuspended        = "tenant.suspended";
    public const string TenantPlanChanged      = "tenant.plan_changed";

    public const string RolePermissionsUpdated = "role.permissions_updated";

    public const string TabCancelled           = "tab.cancelled";
    public const string TabDiscountApplied     = "tab.discount_applied";
    public const string TabServiceFeeApplied   = "tab.service_fee_applied";
    public const string TabTipApplied          = "tab.tip_applied";

    public const string OrderItemCancelled     = "order_item.cancelled";
    public const string OrderItemReturned      = "order_item.returned";

    public const string StockAdjusted          = "stock.adjusted";
    public const string StockWithdrawal        = "stock.withdrawal";

    public const string CashRegisterOpened     = "cash_register.opened";
    public const string CashRegisterClosed     = "cash_register.closed";
    public const string CashWithdrawal         = "cash_register.withdrawal";

    public const string FiscalDocumentCancelled = "fiscal_document.cancelled";

    public const string PrintJobReprinted        = "print_job.reprinted";

    public const string TabCourtesyApplied       = "tab.courtesy_applied";
    public const string ItemDiscountApplied      = "order_item.discount_applied";

    public const string CashSupply               = "cash_register.supply";
}
