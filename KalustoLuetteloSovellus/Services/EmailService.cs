namespace KalustoLuetteloSovellus.Services
{
    using MailKit.Net.Smtp;
    using MimeKit;
    using System.Threading.Tasks;
    public class EmailService
    {
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("your@email.com"));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            email.Body = new TextPart("html")
            {
                Text = body
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.yourserver.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync("your@email.com", "your_password");
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
