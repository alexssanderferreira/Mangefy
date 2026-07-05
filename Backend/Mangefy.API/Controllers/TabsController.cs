using Mangefy.API.Filters;
using Mangefy.Application.Tabs.Commands.CancelOrderItem;
using Mangefy.Application.Tabs.Commands.CancelTab;
using Mangefy.Application.Tabs.Commands.CloseTab;
using Mangefy.Application.Tabs.Commands.MarkItemReady;
using Mangefy.Application.Tabs.Commands.OpenTab;
using Mangefy.Application.Tabs.Commands.ReturnOrderItem;
using Mangefy.Application.Tabs.Commands.StartItemPreparation;
using Mangefy.Application.Tabs.Commands.SubmitOrder;
using Mangefy.Application.Tabs.Queries.GetOpenTabs;
using Mangefy.Application.Tabs.Queries.GetTabById;
using Mangefy.Domain.Roles;
using Mangefy.Domain.Tabs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/[controller]")]
[Authorize]
[ValidateTenantAccess]
public sealed class TabsController : ControllerBase
{
    private readonly ISender _sender;
    public TabsController(ISender sender) => _sender = sender;

    [HttpGet]
    [RequirePermission(PermissionCatalog.Tabs.Read)]
    public async Task<IActionResult> GetOpen(Guid tenantId, CancellationToken ct)
        => Ok(await _sender.Send(new GetOpenTabsQuery(tenantId), ct));

    [HttpGet("{tabId:guid}")]
    [RequirePermission(PermissionCatalog.Tabs.Read)]
    public async Task<IActionResult> GetById(Guid tenantId, Guid tabId, CancellationToken ct)
        => Ok(await _sender.Send(new GetTabByIdQuery(tenantId, tabId), ct));

    [HttpPost]
    [RequirePermission(PermissionCatalog.Tabs.Create)]
    public async Task<IActionResult> Open(Guid tenantId, [FromBody] OpenTabRequest request, CancellationToken ct)
    {
        var deliveryInfo = request.DeliveryInfo is null ? null
            : DeliveryInfo.Create(
                request.DeliveryInfo.RecipientName,
                request.DeliveryInfo.Address,
                request.DeliveryInfo.Complement,
                request.DeliveryInfo.PhoneNumber,
                request.DeliveryInfo.ExternalOrderRef);

        var id = await _sender.Send(new OpenTabCommand(
            tenantId, request.EmployeeId, request.CustomerName,
            request.TableId, request.LocationNote,
            request.Channel, deliveryInfo,
            request.ClientCommandId), ct);
        return Created($"/api/tenants/{tenantId}/tabs/{id}", new { id });
    }

    [HttpPost("{tabId:guid}/orders")]
    [RequirePermission(PermissionCatalog.Orders.Create)]
    public async Task<IActionResult> SubmitOrder(Guid tenantId, Guid tabId, [FromBody] SubmitOrderRequest request, CancellationToken ct)
    {
        var items = request.Items.Select(i => new OrderItemRequest(
            i.MenuItemId, i.Quantity, i.Notes, i.Modifiers)).ToList();

        var orderId = await _sender.Send(new SubmitOrderCommand(tenantId, tabId, request.EmployeeId, items, request.ClientCommandId), ct);
        return Created(string.Empty, new { orderId });
    }

    [HttpPost("{tabId:guid}/close")]
    [RequirePermission(PermissionCatalog.Tabs.Close)]
    public async Task<IActionResult> Close(Guid tenantId, Guid tabId, [FromBody] CloseTabRequest request, CancellationToken ct)
    {
        var payments = request.Payments.Select(p => new PaymentRequest(p.Amount, p.Method, p.ChangeGiven, p.ExternalReference)).ToList();
        await _sender.Send(new CloseTabCommand(tenantId, tabId, payments, request.DiscountAmount, request.DiscountReason, request.ServiceFee, request.Tip), ct);
        return NoContent();
    }

    [HttpPost("{tabId:guid}/cancel")]
    [RequirePermission(PermissionCatalog.Tabs.Cancel)]
    public async Task<IActionResult> Cancel(Guid tenantId, Guid tabId, [FromBody] CancelTabRequest request, CancellationToken ct)
    {
        await _sender.Send(new CancelTabCommand(tenantId, tabId, request.Reason), ct);
        return NoContent();
    }

    // ── KDS ──────────────────────────────────────────────────────────────────

    [HttpPatch("{tabId:guid}/orders/{orderId:guid}/items/{itemId:guid}/start")]
    [RequirePermission(PermissionCatalog.Orders.UpdateStatus)]
    public async Task<IActionResult> StartPreparation(Guid tenantId, Guid tabId, Guid orderId, Guid itemId, CancellationToken ct)
    {
        await _sender.Send(new StartItemPreparationCommand(tenantId, tabId, orderId, itemId), ct);
        return NoContent();
    }

    [HttpPatch("{tabId:guid}/orders/{orderId:guid}/items/{itemId:guid}/ready")]
    [RequirePermission(PermissionCatalog.Orders.UpdateStatus)]
    public async Task<IActionResult> MarkItemReady(Guid tenantId, Guid tabId, Guid orderId, Guid itemId, CancellationToken ct)
    {
        await _sender.Send(new MarkItemReadyCommand(tenantId, tabId, orderId, itemId), ct);
        return NoContent();
    }

    [HttpPatch("{tabId:guid}/orders/{orderId:guid}/items/{itemId:guid}/return")]
    [RequirePermission(PermissionCatalog.Orders.UpdateStatus)]
    public async Task<IActionResult> ReturnItem(Guid tenantId, Guid tabId, Guid orderId, Guid itemId, CancellationToken ct)
    {
        await _sender.Send(new ReturnOrderItemCommand(tenantId, tabId, orderId, itemId), ct);
        return NoContent();
    }

    [HttpPatch("{tabId:guid}/orders/{orderId:guid}/items/{itemId:guid}/cancel")]
    [RequirePermission(PermissionCatalog.Orders.Cancel)]
    public async Task<IActionResult> CancelItem(Guid tenantId, Guid tabId, Guid orderId, Guid itemId,
        [FromBody] CancelOrderItemRequest request, CancellationToken ct)
    {
        await _sender.Send(new CancelOrderItemCommand(tenantId, tabId, orderId, itemId, request.Reason), ct);
        return NoContent();
    }
}

// Request records
public sealed record DeliveryInfoRequest(
    string RecipientName,
    string Address,
    string? Complement = null,
    string? PhoneNumber = null,
    string? ExternalOrderRef = null);

public sealed record OpenTabRequest(
    Guid EmployeeId,
    string CustomerName,
    Guid? TableId,
    string? LocationNote,
    SaleChannel Channel = SaleChannel.InPerson,
    DeliveryInfoRequest? DeliveryInfo = null,
    Guid? ClientCommandId = null);

public sealed record SubmitOrderItemRequest(
    Guid MenuItemId,
    int Quantity,
    string? Notes = null,
    IReadOnlyList<string>? Modifiers = null);

public sealed record SubmitOrderRequest(
    Guid EmployeeId,
    IReadOnlyList<SubmitOrderItemRequest> Items,
    Guid? ClientCommandId = null);

public sealed record ClosePaymentRequest(
    decimal Amount,
    PaymentMethod Method,
    decimal ChangeGiven = 0m,
    string? ExternalReference = null);

public sealed record CloseTabRequest(
    IReadOnlyList<ClosePaymentRequest> Payments,
    decimal DiscountAmount = 0m,
    string? DiscountReason = null,
    decimal ServiceFee = 0m,
    decimal Tip = 0m);

public sealed record CancelTabRequest(string Reason);
public sealed record CancelOrderItemRequest(string? Reason);
