using Mangefy.Domain.Common;
using Mangefy.Domain.Common.ValueObjects;
using Mangefy.Domain.Reservations.Events;

namespace Mangefy.Domain.Reservations;

public sealed class Reservation : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public string CustomerName { get; private set; }
    public PhoneNumber? CustomerPhone { get; private set; }
    public int PartySize { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeOnly Time { get; private set; }
    public Guid? TableId { get; private set; }
    public string? Notes { get; private set; }
    public string? CancellationReason { get; private set; }
    public ReservationStatus Status { get; private set; }

    /// <summary>
    /// Comanda aberta ao confirmar chegada do cliente. Nulo até o cliente chegar.
    /// </summary>
    public Guid? TabId { get; private set; }

    private Reservation() { }

    public static Reservation Create(
        Guid tenantId,
        string customerName,
        int partySize,
        DateOnly date,
        TimeOnly time,
        Guid employeeId,
        Guid? tableId = null,
        string? customerPhone = null,
        string? notes = null)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("Nome do cliente não pode ser vazio.");

        if (partySize < 1)
            throw new DomainException("Número de pessoas deve ser maior que zero.");

        if (date < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new DomainException("Data da reserva não pode ser no passado.");

        var reservation = new Reservation
        {
            TenantId = tenantId,
            CustomerName = customerName.Trim(),
            CustomerPhone = customerPhone is not null ? PhoneNumber.Create(customerPhone) : null,
            PartySize = partySize,
            Date = date,
            Time = time,
            TableId = tableId,
            Notes = notes?.Trim(),
            Status = ReservationStatus.Pending,
            CreatedByEmployeeId = employeeId  // herdado de Entity
        };

        reservation.AddDomainEvent(new ReservationCreatedEvent(reservation.Id, tenantId, date, time, tableId));
        return reservation;
    }

    public void Confirm()
    {
        if (Status != ReservationStatus.Pending)
            throw new DomainException("Apenas reservas pendentes podem ser confirmadas.");

        Status = ReservationStatus.Confirmed;
        SetUpdatedAt();
    }

    public void RegisterArrival(Guid tabId)
    {
        if (Status is not (ReservationStatus.Pending or ReservationStatus.Confirmed))
            throw new DomainException("Reserva não pode registrar chegada no status atual.");

        Status = ReservationStatus.Arrived;
        TabId = tabId;
        SetUpdatedAt();
        AddDomainEvent(new ReservationArrivedEvent(Id, TenantId, tabId));
    }

    public void Cancel(string reason)
    {
        if (Status is ReservationStatus.Arrived or ReservationStatus.Cancelled)
            throw new DomainException("Reserva não pode ser cancelada no status atual.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Motivo do cancelamento não pode ser vazio.");

        Status = ReservationStatus.Cancelled;
        CancellationReason = reason.Trim();
        SetUpdatedAt();
    }

    public void MarkAsNoShow()
    {
        if (Status is not (ReservationStatus.Pending or ReservationStatus.Confirmed))
            throw new DomainException("Apenas reservas pendentes ou confirmadas podem ser marcadas como no-show.");

        Status = ReservationStatus.NoShow;
        SetUpdatedAt();
    }

    public void UpdateDetails(
        string customerName, int partySize, DateOnly date, TimeOnly time, Guid? tableId, string? notes, string? customerPhone)
    {
        if (Status is ReservationStatus.Arrived or ReservationStatus.Cancelled or ReservationStatus.NoShow)
            throw new DomainException("Reserva finalizada não pode ser editada.");

        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("Nome do cliente não pode ser vazio.");

        if (partySize < 1)
            throw new DomainException("Número de pessoas deve ser maior que zero.");

        CustomerName = customerName.Trim();
        CustomerPhone = customerPhone is not null ? PhoneNumber.Create(customerPhone) : null;
        PartySize = partySize;
        Date = date;
        Time = time;
        TableId = tableId;
        Notes = notes?.Trim();
        SetUpdatedAt();
    }
}
