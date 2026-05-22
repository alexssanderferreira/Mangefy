using System.Net.Http.Headers;
using System.Net.Http.Json;
using Mangefy.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mangefy.Infrastructure.Services.Email;

public sealed class ResendEmailSender : IEmailSender
{
    private readonly HttpClient _http;
    private readonly ResendOptions _options;
    private readonly ILogger<ResendEmailSender> _logger;

    public ResendEmailSender(HttpClient http, IOptions<ResendOptions> options, ILogger<ResendEmailSender> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.ApiKey) || _options.ApiKey.StartsWith("SET_"))
        {
            _logger.LogWarning("Resend desabilitado ou sem ApiKey. Email para {To} não enviado. Assunto: {Subject}", to, subject);
            return false;
        }

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        var payload = new
        {
            from = $"{_options.FromName} <{_options.FromEmail}>",
            to = new[] { to },
            subject,
            html = htmlBody
        };

        try
        {
            var resp = await _http.PostAsJsonAsync("https://api.resend.com/emails", payload, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                _logger.LogError("Falha ao enviar email via Resend ({Status}): {Body}", resp.StatusCode, body);
                return false;
            }

            _logger.LogInformation("Email enviado via Resend para {To}", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email via Resend para {To}", to);
            return false;
        }
    }

    public Task<bool> SendOwnerActivationAsync(string toEmail, string ownerName, string activationToken, CancellationToken ct = default)
    {
        var baseUrl = (_options.AppBaseUrl ?? "http://localhost:4200").TrimEnd('/');
        var link = $"{baseUrl}/auth/activate-owner?token={activationToken}";
        var html = BuildHtml(ownerName, link);
        return SendAsync(toEmail, "Ative sua conta Mangefy", html, ct);
    }

    private static string BuildHtml(string ownerName, string link) => $@"
<!DOCTYPE html>
<html><body style=""font-family: -apple-system, Segoe UI, Roboto, sans-serif; background:#f5f5f7; padding:24px;"">
  <div style=""max-width:480px; margin:0 auto; background:#fff; border-radius:12px; padding:32px; box-shadow:0 2px 8px rgba(0,0,0,.05);"">
    <h1 style=""margin:0 0 12px; font-size:22px; color:#0a0a0a;"">Bem-vindo à Mangefy 🍽</h1>
    <p style=""color:#444; line-height:1.6;"">Olá <strong>{System.Net.WebUtility.HtmlEncode(ownerName)}</strong>,</p>
    <p style=""color:#444; line-height:1.6;"">Sua conta de dono foi criada. Para começar a gerenciar seus estabelecimentos, defina uma senha clicando no botão abaixo:</p>
    <p style=""text-align:center; margin:28px 0;"">
      <a href=""{link}"" style=""display:inline-block; background:#0a0a0a; color:#f5c400; padding:12px 28px; border-radius:8px; text-decoration:none; font-weight:700;"">Ativar minha conta</a>
    </p>
    <p style=""color:#888; font-size:12px; line-height:1.5;"">Este link é válido por <strong>48 horas</strong>. Se você não solicitou esta conta, pode ignorar este e-mail.</p>
    <p style=""color:#aaa; font-size:11px; word-break:break-all;"">Ou copie e cole: {link}</p>
  </div>
</body></html>";
}
