using Mangefy.Domain.Common;

namespace Mangefy.Domain.Devices;

/// <summary>
/// Dispositivo registrado no tenant. Preparatório para rastreamento de sessão
/// operacional, idempotência por dispositivo e sincronização offline futura.
/// </summary>
public sealed class Device : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DeviceType Type { get; private set; }

    /// <summary>
    /// Identificador público do dispositivo (ex.: UUID gerado pelo app, MAC, serial).
    /// Usado para correlacionar requisições ao mesmo dispositivo físico.
    /// </summary>
    public string PublicIdentifier { get; private set; } = string.Empty;

    public DeviceStatus Status { get; private set; }
    public DateTime? LastSeenAt { get; private set; }

    private Device() { }

    public static Device Register(Guid tenantId, string name, DeviceType type, string publicIdentifier)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do dispositivo não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(publicIdentifier))
            throw new DomainException("Identificador público do dispositivo não pode ser vazio.");

        return new Device
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Type = type,
            PublicIdentifier = publicIdentifier.Trim(),
            Status = DeviceStatus.Active
        };
    }

    public void UpdateInfo(string name, DeviceType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do dispositivo não pode ser vazio.");

        Name = name.Trim();
        Type = type;
        SetUpdatedAt();
    }

    public void RecordActivity()
    {
        LastSeenAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        Status = DeviceStatus.Inactive;
        SetUpdatedAt();
    }

    public void Reactivate()
    {
        Status = DeviceStatus.Active;
        SetUpdatedAt();
    }
}
