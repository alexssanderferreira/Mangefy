namespace Mangefy.Application.Common.Interfaces;

public interface IAdminSaasCredentials
{
    string Email { get; }
    string PasswordHash { get; }
}
