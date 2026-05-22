namespace Mangefy.Domain.Tables;

public enum TableStatus
{
    Available,    // livre — sem comandas abertas
    Occupied,     // ocupada — tem pelo menos uma comanda aberta
    Reserved,     // reservada
    Unavailable   // indisponível (manutenção)
}
