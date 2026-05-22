using Mangefy.Domain.Common;

namespace Mangefy.Domain.Settings;

public sealed class PrinterSettings : AggregateRoot
{
    public Guid TenantId { get; private set; }

    private readonly List<Printer> _printers = [];
    public IReadOnlyList<Printer> Printers => _printers.AsReadOnly();

    private PrinterSettings() { }

    public static PrinterSettings Create(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        return new PrinterSettings { TenantId = tenantId };
    }

    public Printer AddPrinter(string name, string ipAddressOrPort, PrinterStation station)
    {
        var printer = Printer.Create(name, ipAddressOrPort, station);

        // First printer per station becomes default automatically
        if (!_printers.Any(p => p.Station == station && p.IsActive))
            printer.SetAsDefault();

        _printers.Add(printer);
        SetUpdatedAt();
        return printer;
    }

    public void UpdatePrinter(Guid printerId, string name, string ipAddressOrPort, PrinterStation station)
    {
        var printer = GetPrinterOrThrow(printerId);
        printer.Update(name, ipAddressOrPort, station);
        SetUpdatedAt();
    }

    public void SetDefaultPrinter(Guid printerId)
    {
        var target = GetPrinterOrThrow(printerId);

        if (!target.IsActive)
            throw new DomainException("Impressora inativa não pode ser padrão.");

        foreach (var p in _printers.Where(p => p.Station == target.Station))
            p.UnsetDefault();

        target.SetAsDefault();
        SetUpdatedAt();
    }

    public void RemovePrinter(Guid printerId)
    {
        var printer = GetPrinterOrThrow(printerId);
        printer.Deactivate();

        // Assign a new default for the station if needed
        if (printer.IsDefault)
        {
            var next = _printers.FirstOrDefault(p => p.Id != printerId && p.Station == printer.Station && p.IsActive);
            next?.SetAsDefault();
        }

        SetUpdatedAt();
    }

    public Printer? GetDefaultForStation(PrinterStation station) =>
        _printers.FirstOrDefault(p => p.Station == station && p.IsDefault && p.IsActive);

    private Printer GetPrinterOrThrow(Guid printerId) =>
        _printers.FirstOrDefault(p => p.Id == printerId)
        ?? throw new DomainException("Impressora não encontrada.");
}
