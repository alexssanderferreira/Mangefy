namespace Mangefy.Domain.Tabs;

public enum TabStatus
{
    Open,      // comanda aberta, aceitando pedidos
    Closed,    // conta paga e fechada
    Cancelled  // cancelada (ex: cliente foi embora sem pagar — registrado para controle)
}
