using Mangefy.Domain.Common;

namespace Mangefy.Domain.Settings;

/// <summary>
/// Configurações de reserva do tenant.
/// O Owner define o limite de reservas simultâneas (nulo = ilimitado).
/// </summary>
public sealed class ReservationSettings : AggregateRoot
{
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Quantidade máxima de reservas ativas simultaneamente. Nulo = sem limite.
    /// </summary>
    public int? MaxSimultaneousReservations { get; private set; }

    private ReservationSettings() { }

    public static ReservationSettings CreateDefault(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        return new ReservationSettings
        {
            TenantId = tenantId,
            MaxSimultaneousReservations = null
        };
    }

    public void UpdateLimit(int? max)
    {
        if (max.HasValue && max.Value < 1)
            throw new DomainException("O limite de reservas simultâneas deve ser maior que zero.");

        MaxSimultaneousReservations = max;
        SetUpdatedAt();
    }
}
