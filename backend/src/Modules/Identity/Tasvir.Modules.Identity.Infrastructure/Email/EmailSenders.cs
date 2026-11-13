using System.Net.Http.Json;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using Tasvir.Modules.Identity.Application.Abstractions;

namespace Tasvir.Modules.Identity.Infrastructure.Email;

public sealed class SmtpOptions
{
    public const string Section = "Identity:Smtp";

    public string? Host { get; set; }
    public int Port { get; set; } = 587;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string FromAddress { get; set; } = "no-reply@tasvir.uz";
    public string FromName { get; set; } = "Tasvir.uz";
}

internal sealed class SmtpEmailSender(SmtpOptions smtp) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(smtp.FromName, smtp.FromAddress));
        mime.To.Add(MailboxAddress.Parse(message.To));
        mime.Subject = message.Subject;
        mime.Body = new TextPart("plain") { Text = message.TextBody };

        using var client = new SmtpClient();
        // Parolni tiklash havolalari ochiq kanal orqali ketmasligi uchun TLS majburiy
        var security = smtp.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
        await client.ConnectAsync(smtp.Host!, smtp.Port, security, ct);
        if (!string.IsNullOrEmpty(smtp.Username))
        {
            await client.AuthenticateAsync(smtp.Username, smtp.Password ?? string.Empty, ct);
        }

        await client.SendAsync(mime, ct);
        await client.DisconnectAsync(quit: true, ct);
    }
}

/// <summary>
/// Brevo (sendinblue) tranzaksion xat API si: HTTPS orqali ishlaydi. SMTP portlari yopiq xostinglar
/// (masalan, Render bepul tarifi) uchun. Jo‘natuvchi manzil (Identity:Smtp:FromAddress) Brevo da tasdiqlangan bo‘lishi kerak.
/// </summary>
public sealed class BrevoOptions
{
    public const string Section = "Identity:Brevo";

    public string? ApiKey { get; set; }
    public string Endpoint { get; set; } = "https://api.brevo.com/v3/smtp/email";
}

internal sealed class BrevoEmailSender(HttpClient http, BrevoOptions brevo, SmtpOptions sender) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, brevo.Endpoint)
        {
            Content = JsonContent.Create(new
            {
                sender = new { name = sender.FromName, email = sender.FromAddress },
                to = new[] { new { email = message.To } },
                subject = message.Subject,
                textContent = message.TextBody,
            }),
        };
        request.Headers.Add("api-key", brevo.ApiKey);
        request.Headers.Accept.ParseAdd("application/json");

        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            // Javob matnida kalit bo‘lmaydi; xato sababini logda ko‘rish uchun
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Brevo xat yubormadi: {(int)response.StatusCode} {body}");
        }
    }
}

/// <summary>Faqat lokal ishlab chiqish uchun: xat yuborilmaydi, matni (havola bilan) logga yoziladi.</summary>
internal sealed class DevelopmentEmailSender(ILogger<DevelopmentEmailSender> logger) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        logger.LogInformation("[DEV EMAIL] Kimga: {To}; Mavzu: {Subject}\n{Body}", message.To, message.Subject, message.TextBody);
        return Task.CompletedTask;
    }
}
