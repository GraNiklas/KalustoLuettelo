using Humanizer;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Threading.Tasks;
public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}
public class EmailService: IEmailService
{
    private readonly SmtpSettings _settings;

    public EmailService(IOptions<SmtpSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_settings.Username));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart("html") { Text = body };

        using var smtp = new SmtpClient();

        // TÄMÄ PITÄÄ OTTAA POIS KUN SOVELLUS ON VALMIS VAIN TESTAUKSEEN!!
        smtp.ServerCertificateValidationCallback = (s, c, h, e) => true; // TÄMÄ PITÄÄ OTTAA POIS KUN SOVELLUS ON VALMIS, VAIN TESTAUKSEEN!!
        // TÄMÄ PITÄÄ OTTAA POIS KUN SOVELLUS ON VALMIS VAIN TESTAUKSEEN!!

        await smtp.ConnectAsync(_settings.Server, _settings.Port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
