namespace Mangefy.Domain.Tabs;

public enum OrderStatus
{
    Open,        // rascunho — garçom ainda está adicionando itens
    Submitted,   // enviado para a cozinha
    InProgress,  // pelo menos um item em preparo
    Ready,       // todos os itens prontos para servir
    Delivered,   // todos os itens entregues
    Cancelled    // cancelado
}
