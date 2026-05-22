using Mangefy.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Mangefy.Infrastructure.Auth;

public sealed class AdminSaasCredentials : IAdminSaasCredentials
{
    public string Email { get; }
    public string PasswordHash { get; }

    public AdminSaasCredentials(IConfiguration configuration)
    {
        Email = configuration["AdminSaas:Email"]
            ?? throw new InvalidOperationException("AdminSaas:Email is not configured.");
        PasswordHash = configuration["AdminSaas:PasswordHash"]
            ?? throw new InvalidOperationException("AdminSaas:PasswordHash is not configured.");
    }
}
