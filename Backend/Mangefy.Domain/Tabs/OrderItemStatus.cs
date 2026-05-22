namespace Mangefy.Domain.Tabs;

public enum OrderItemStatus
{
    Pending,    // aguardando envio para cozinha
    Sent,       // no KDS — aguardando cozinha aceitar
    Preparing,  // sendo preparado
    Ready,      // pronto para servir
    Delivered,  // entregue ao cliente
    Returned,   // devolvido à cozinha após entrega
    Cancelled   // cancelado
}
