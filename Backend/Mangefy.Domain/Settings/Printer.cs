using Mangefy.Domain.Common;

namespace Mangefy.Domain.Settings;

public enum PrinterStation
{
    Cashier,
    Kitchen,
    Bar,
    Custom
}

public sealed class Printer : Entity
{
    public string Name { get; private set; }
    public string IpAddressOrPort { get; private set; }
    public PrinterStation Station { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }

    private Printer() { }

    internal static Printer Create(string name, string ipAddressOrPort, PrinterStation station)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da impressora não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(ipAddressOrPort))
            throw new DomainException("Endereço da impressora não pode ser vazio.");

        return new Printer
        {
            Name = name.Trim(),
            IpAddressOrPort = ipAddressOrPort.Trim(),
            Station = station,
            IsDefault = false,
            IsActive = true
        };
    }

    internal void Update(string name, string ipAddressOrPort, PrinterStation station)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da impressora não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(ipAddressOrPort))
            throw new DomainException("Endereço da impressora não pode ser vazio.");

        Name = name.Trim();
        IpAddressOrPort = ipAddressOrPort.Trim();
        Station = station;
        SetUpdatedAt();
    }

    internal void SetAsDefault() { IsDefault = true; SetUpdatedAt(); }
    internal void UnsetDefault() { IsDefault = false; SetUpdatedAt(); }
    internal void Deactivate() { IsActive = false; IsDefault = false; SetUpdatedAt(); }
    internal void Activate() { IsActive = true; SetUpdatedAt(); }
}
