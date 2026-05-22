using Mangefy.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Mangefy.API.Filters;

/// <summary>
/// Garante que o tenantId da URL coincide com o tenantId do token JWT do usuário autenticado.
/// AdminSaas (IsAdminSaas=true) é isento — eles têm controle próprio via RequireAdminSaasAttribute.
/// Aplique em todos os controllers com rota api/tenants/{tenantId:guid}/...
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ValidateTenantAccessAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var currentUser = context.HttpContext.RequestServices.GetRequiredService<ICurrentUser>();

        if (currentUser.IsAdminSaas)
        {
            await next();
            return;
        }

        if (!context.ActionArguments.TryGetValue("tenantId", out var routeValue)
            || routeValue is not Guid routeTenantId)
        {
            context.Result = new ForbidResult();
            return;
        }

        if (currentUser.TenantId is null || currentUser.TenantId.Value != routeTenantId)
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
