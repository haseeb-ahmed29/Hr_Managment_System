using HrMangmentSystem_Application.Config;
using HrMangmentSystem_Application.Interfaces.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace HrMangmentSystem_Application.Implementation.Notifications
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _smtpOptions;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
        {
            _smtpOptions = options.Value;
            _logger = logger;
        }
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var message = new MailMessage();
                message.From = new MailAddress(_smtpOptions.From);
                message.To.Add(new MailAddress(to));
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using var client = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port)
                {
                    Credentials = new NetworkCredential(_smtpOptions.UserName, _smtpOptions.Password),
                    EnableSsl = _smtpOptions.EnableSsl
                };

                await client.SendMailAsync(message);

                _logger.LogInformation("Email sent to {To} with subject {Subject}", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                     "Error while sending email to {To} with subject {Subject}", to, subject);
            }
        }
    }
}
