namespace Mangefy.Infrastructure.Services.Email;

public sealed class ResendOptions
{
    public const string SectionName = "Resend";
    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "onboarding@resend.dev";
    public string FromName { get; set; } = "Mangefy";
    public string AppBaseUrl { get; set; } = "http://localhost:4200";
    public bool Enabled { get; set; } = true;
}
