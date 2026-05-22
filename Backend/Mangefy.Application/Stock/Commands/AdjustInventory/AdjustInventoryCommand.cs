using MediatR;

namespace Mangefy.Application.Stock.Commands.AdjustInventory;

public sealed record AdjustInventoryCommand(
    Guid TenantId,
    Guid StockItemId,
    decimal NewQuantity,
    string Reason) : IRequest;
