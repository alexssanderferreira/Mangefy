using Mangefy.Domain.Common;
using Mangefy.Domain.Tables.Events;

namespace Mangefy.Domain.Tables;

public sealed class Table : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public string Number { get; private set; }
    public int Capacity { get; private set; }
    public TableStatus Status { get; private set; }
    public string? Section { get; private set; }  // setor: "Área interna", "Varanda", "Balcão"

    private Table() { }

    public static Table Create(Guid tenantId, string number, int capacity, string? section = null)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId inválido.");

        if (string.IsNullOrWhiteSpace(number))
            throw new DomainException("Número da mesa não pode ser vazio.");

        if (capacity <= 0)
            throw new DomainException("Capacidade da mesa deve ser maior que zero.");

        return new Table
        {
            TenantId = tenantId,
            Number = number.Trim(),
            Capacity = capacity,
            Section = section?.Trim(),
            Status = TableStatus.Available
        };
    }

    /// <summary>
    /// Marca a mesa como Ocupada quando a primeira comanda é aberta nela.
    /// Chamado pela Application layer via TabOpenedEvent.
    /// </summary>
    public void Occupy()
    {
        if (Status == TableStatus.Unavailable)
            throw new DomainException($"Mesa {Number} está indisponível.");

        if (Status == TableStatus.Occupied)
            return; // múltiplas comandas na mesma mesa — idempotente

        Status = TableStatus.Occupied;
        SetUpdatedAt();
        AddDomainEvent(new TableOccupiedEvent(Id, TenantId, Number));
    }

    /// <summary>
    /// Libera a mesa quando a última comanda aberta é fechada ou cancelada.
    /// Chamado pela Application layer via TabClosedEvent / TabCancelledEvent.
    /// </summary>
    public void Release()
    {
        if (Status != TableStatus.Occupied)
            return; // idempotente — pode já estar available se todas comandas foram encerradas

        Status = TableStatus.Available;
        SetUpdatedAt();
        AddDomainEvent(new TableReleasedEvent(Id, TenantId, Number));
    }

    public void Reserve()
    {
        if (Status != TableStatus.Available)
            throw new DomainException($"Mesa {Number} não está disponível para reserva.");

        Status = TableStatus.Reserved;
        SetUpdatedAt();
    }

    public void CancelReservation()
    {
        if (Status != TableStatus.Reserved)
            throw new DomainException($"Mesa {Number} não está reservada.");

        Status = TableStatus.Available;
        SetUpdatedAt();
    }

    public void MarkAsUnavailable()
    {
        if (Status == TableStatus.Occupied)
            throw new DomainException($"Mesa {Number} está ocupada e não pode ser desativada.");

        Status = TableStatus.Unavailable;
        SetUpdatedAt();
    }

    public void UpdateInfo(string number, int capacity, string? section)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new DomainException("Número da mesa não pode ser vazio.");

        if (capacity <= 0)
            throw new DomainException("Capacidade da mesa deve ser maior que zero.");

        Number = number.Trim();
        Capacity = capacity;
        Section = section?.Trim();
        SetUpdatedAt();
    }

    public bool IsAvailable() => Status == TableStatus.Available;
}
