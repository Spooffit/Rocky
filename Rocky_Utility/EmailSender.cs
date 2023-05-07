using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;

namespace Rocky_Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private EmailSMTP _emailSTMP;
        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
            _emailSTMP = new EmailSMTP()
            {
                EmailSender = _configuration["EmailSMTP_EmailSender"] ?? "",
                AppPassword = _configuration["EmailSMTP_AppPassword"] ?? ""
            };
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(email,subject,htmlMessage);
        }

        public async Task Execute(string email, string subject, string body)
        {

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_emailSTMP.EmailSender));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = body
            };

            using var stmp = new SmtpClient();
            stmp.Connect("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect);
            stmp.Authenticate(WC.EmailDomain, _emailSTMP.AppPassword);
            stmp.Send(message);
            stmp.Disconnect(true);
        }
    }
}
