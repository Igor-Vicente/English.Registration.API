using System.Net;
using System.Net.Mail;

namespace English.Registration.API.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
    public class EmailSender : IEmailSender
    {
        private readonly string _domainEmail;
        private readonly string _passwordEmail;
        public EmailSender(IConfiguration configuration)
        {
            _domainEmail = configuration["MailingProvider:Email"] ?? throw new ArgumentNullException("MailingProvider:Email not provided");
            _passwordEmail = configuration["MailingProvider:Password"] ?? throw new ArgumentNullException("MailingProvider:Password not provided");
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            using (var client = new SmtpClient("smtp.zoho.com", 587))
            {
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_domainEmail, _passwordEmail);

                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(_domainEmail);
                    mailMessage.To.Add(email);
                    mailMessage.Subject = subject;
                    mailMessage.Body = message;
                    mailMessage.IsBodyHtml = true;

                    await client.SendMailAsync(mailMessage);
                }
            }
        }
    }
}
