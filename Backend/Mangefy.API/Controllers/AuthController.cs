using Mangefy.API.Filters;
using Mangefy.Application.Auth.Commands.ActivateOwnerAccount;
using Mangefy.Application.Auth.Commands.AdminSaasLogin;
using Mangefy.Application.Auth.Commands.Login;
using Mangefy.Application.Auth.Commands.ResolveTenants;
using Mangefy.Application.Auth.Commands.SetPassword;
using Mangefy.Application.Auth.Commands.SwitchTenant;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mangefy.API.Controllers;

public sealed record SetPasswordRequest(string Token, string NewPassword);
public sealed record ActivateOwnerAccountRequest(string Token, string NewPassword);
public sealed record ResolveTenantsRequest(string Email, string Password);
public sealed record SwitchTenantRequest(string TargetTenantSlug);

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;
    public AuthController(ISender sender) => _sender = sender;

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
        => Ok(await _sender.Send(command, ct));

    /// <summary>
    /// Resolve quais tenants um usuário tem acesso dado email + senha.
    /// Usado antes do login para exibir a tela de seleção de estabelecimento.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("resolve-tenants")]
    public async Task<IActionResult> ResolveTenants([FromBody] ResolveTenantsRequest req, CancellationToken ct)
        => Ok(await _sender.Send(new ResolveTenantsCommand(req.Email, req.Password), ct));

    /// <summary>
    /// Troca o tenant ativo sem redigitar senha. Requer JWT válido.
    /// </summary>
    [Authorize]
    [HttpPost("switch-tenant")]
    public async Task<IActionResult> SwitchTenant([FromBody] SwitchTenantRequest req, CancellationToken ct)
        => Ok(await _sender.Send(new SwitchTenantCommand(req.TargetTenantSlug), ct));

    [AllowAnonymous]
    [HttpPost("set-password")]
    public async Task<IActionResult> SetPassword([FromBody] SetPasswordRequest request, CancellationToken ct)
    {
        await _sender.Send(new SetPasswordCommand(request.Token, request.NewPassword), ct);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("owner/activate")]
    public async Task<IActionResult> ActivateOwnerAccount([FromBody] ActivateOwnerAccountRequest request, CancellationToken ct)
    {
        await _sender.Send(new ActivateOwnerAccountCommand(request.Token, request.NewPassword), ct);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("admin/login")]
    public async Task<IActionResult> AdminSaasLogin([FromBody] AdminSaasLoginCommand command, CancellationToken ct)
        => Ok(await _sender.Send(command, ct));
}
