namespace Mangefy.Domain.Platform.Features;

/// <summary>
/// Catálogo imutável de todas as funcionalidades da plataforma.
/// Definido no código pelo Mangefy — não gerenciável por tenants.
/// O AdminSaas usa essas chaves para configurar o PlanFeatureSet (matriz Plano × Tipo de Negócio).
/// </summary>
public static class FeatureCatalog
{
    public static class Orders
    {
        public const string Kds = "features.kds";  // tela Kitchen Display System
    }

    public static class Tabs
    {
        public const string Management = "features.tabs"; // gestão de comandas
    }

    public static class Menu
    {
        public const string MultiMenu = "features.multi_menu"; // múltiplos cardápios (café, almoço...)
    }

    public static class Stock
    {
        public const string Basic    = "features.stock_basic";    // controle básico de estoque
        public const string Advanced = "features.stock_advanced"; // estoque avançado com alertas
    }

    public static class Cash
    {
        public const string DailyClose = "features.daily_cash"; // fechamento de caixa diário
    }

    public static class Reports
    {
        public const string Basic    = "features.reports_basic";    // relatórios essenciais
        public const string Advanced = "features.reports_advanced"; // analytics avançado
    }

    public static class Reservations
    {
        public const string Management = "features.reservations"; // gestão de reservas de mesa
    }

    public static class Delivery
    {
        public const string Module = "features.delivery"; // módulo de delivery
    }

    public static class Roles
    {
        public const string CustomRoles = "features.custom_roles"; // criação de cargos customizados
    }

    /// <summary>
    /// Conjunto de todas as feature keys válidas da plataforma.
    /// </summary>
    public static IReadOnlySet<string> All { get; } = new HashSet<string>
    {
        Orders.Kds,
        Tabs.Management,
        Menu.MultiMenu,
        Stock.Basic, Stock.Advanced,
        Cash.DailyClose,
        Reports.Basic, Reports.Advanced,
        Reservations.Management,
        Delivery.Module,
        Roles.CustomRoles
    };

    public static bool IsValid(string feature) => All.Contains(feature);
}
