namespace Mangefy.Domain.Roles;

/// <summary>
/// Catálogo de permissões da plataforma. Imutável — definido pelo Mangefy, não pelo tenant.
/// Cada string é a chave usada para persistência e verificação em runtime.
/// </summary>
public static class PermissionCatalog
{
    public static class Orders
    {
        public const string Read         = "orders.read";
        public const string Create       = "orders.create";
        public const string UpdateStatus = "orders.update_status";
        public const string Cancel       = "orders.cancel";

        /// <summary>Cancelar item já enviado para a cozinha (Sent/Preparing). Motivo obrigatório.</summary>
        public const string CancelAfterSent = "orders.cancel_after_sent";

        /// <summary>Cancelar item em preparo ou pronto (Preparing/Ready). Motivo obrigatório.</summary>
        public const string CancelInPreparation = "orders.cancel_in_preparation";

        /// <summary>Cancelar item já entregue (Delivered). Requer motivo obrigatório. Permissão gerencial.</summary>
        public const string CancelDelivered = "orders.cancel_delivered";
    }

    public static class Tabs
    {
        public const string Read   = "tabs.read";
        public const string Create = "tabs.create";
        public const string Close  = "tabs.close";
        public const string Cancel = "tabs.cancel";

        /// <summary>Aplicar desconto em item ou comanda até o limite configurado em TabSettings.</summary>
        public const string ApplyDiscount = "tabs.apply_discount";

        /// <summary>Aplicar desconto acima do limite configurado ou cortesia. Permissão gerencial.</summary>
        public const string ApplyDiscountOverride = "tabs.apply_discount_override";

        /// <summary>Aplicar cortesia (comanda gratuita ou item gratuito).</summary>
        public const string ApplyCourtesy = "tabs.apply_courtesy";
    }

    public static class Menu
    {
        public const string Read   = "menu.read";
        public const string Manage = "menu.manage";
    }

    public static class Tables
    {
        public const string Read   = "tables.read";
        public const string Manage = "tables.manage";
    }

    public static class Employees
    {
        public const string Read   = "employees.read";
        public const string Manage = "employees.manage";
    }

    public static class Roles
    {
        public const string Read   = "roles.read";
        public const string Manage = "roles.manage";
    }

    public static class Stock
    {
        public const string Read   = "stock.read";
        public const string Manage = "stock.manage";
    }

    public static class Cash
    {
        public const string Manage = "cash.manage";
    }

    public static class Reservations
    {
        public const string Read   = "reservations.read";
        public const string Manage = "reservations.manage";
    }

    public static class Reports
    {
        public const string Read = "reports.read";
    }

    public static class Settings
    {
        public const string Manage = "settings.manage";
    }

    public static class Sessions
    {
        /// <summary>Iniciar e encerrar sessão operacional (ponto eletrônico).</summary>
        public const string Manage = "sessions.manage";
    }

    /// <summary>
    /// Todas as permissões disponíveis na plataforma.
    /// </summary>
    public static IReadOnlySet<string> All { get; } = new HashSet<string>
    {
        Orders.Read, Orders.Create, Orders.UpdateStatus, Orders.Cancel,
        Orders.CancelAfterSent, Orders.CancelInPreparation, Orders.CancelDelivered,
        Tabs.Read, Tabs.Create, Tabs.Close, Tabs.Cancel,
        Tabs.ApplyDiscount, Tabs.ApplyDiscountOverride, Tabs.ApplyCourtesy,
        Menu.Read, Menu.Manage,
        Tables.Read, Tables.Manage,
        Stock.Read, Stock.Manage,
        Cash.Manage,
        Reservations.Read, Reservations.Manage,
        Employees.Read, Employees.Manage,
        Roles.Read, Roles.Manage,
        Reports.Read,
        Settings.Manage,
        Sessions.Manage
    };

    public static bool IsValid(string permission) => All.Contains(permission);
}
