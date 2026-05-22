using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using MediatR;

namespace Mangefy.Application.Auth.Commands.AdminSaasLogin;

public sealed class AdminSaasLoginCommandHandler : IRequestHandler<AdminSaasLoginCommand, AdminSaasLoginResult>
{
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAdminSaasCredentials _credentials;

    public AdminSaasLoginCommandHandler(
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IAdminSaasCredentials credentials)
    {
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _credentials = credentials;
    }

    public Task<AdminSaasLoginResult> Handle(AdminSaasLoginCommand request, CancellationToken cancellationToken)
    {
        if (!string.Equals(request.Email, _credentials.Email, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenException("Credenciais inválidas.");

        if (!_passwordHasher.Verify(request.Password, _credentials.PasswordHash))
            throw new ForbiddenException("Credenciais inválidas.");

        var token = _tokenService.GenerateAdminSaasToken(request.Email);

        return Task.FromResult(new AdminSaasLoginResult(token.AccessToken, token.ExpiresAt, request.Email));
    }
}
