namespace Mangefy.Application.Common.Interfaces;

public sealed record TokenResult(string AccessToken, DateTime ExpiresAt);

public interface ITokenService
{
    TokenResult GenerateToken(Guid employeeId, Guid tenantId, string email, IEnumerable<string> permissions);
    TokenResult GenerateOwnerTenantToken(Guid ownerId, Guid tenantId, string email, IEnumerable<string> permissions);
    TokenResult GenerateAdminSaasToken(string email);
}
