using English.Registration.API.Services;

namespace English.Registration.API.Extensions
{
    public static class MailingExtensions
    {
        public static Task SendEmailResetPasswordAsync(this IEmailSender emailSender, string email, string link)
        {
            return emailSender.SendEmailAsync(email, "Reset Password",
               $"You can click <a href='{link}'>here</a> to reset your password. <br /><br />" +
               $"If you didn’t ask to reset your password, you can ignore this message. <br /> Thanks, good studies 👋\r\n");
        }
    }
}
