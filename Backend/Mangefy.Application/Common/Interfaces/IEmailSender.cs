namespace Mangefy.Application.Common.Interfaces;

public interface IEmailSender
{
    Task<bool> SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default);

    /// <summary>
    /// Envia o e-mail de ativação de conta de owner com o link de ativação composto a partir do AppBaseUrl configurado.
    /// Retorna true se entregue (200 da API de email) ou false caso contrário (sem ApiKey, erro, etc).
    /// </summary>
    Task<bool> SendOwnerActivationAsync(string toEmail, string ownerName, string activationToken, CancellationToken ct = default);
}
