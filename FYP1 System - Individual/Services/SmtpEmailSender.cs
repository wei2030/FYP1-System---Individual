using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace FYP1_System___Individual.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public SmtpEmailSender(IConfiguration config)
        {
            _config = config;
        }

        //public async Task SendEmailAsync(string toEmail, string subject, string body)
        //{
        //    var smtpHost = _config["Smtp:Host"];
        //    var smtpPort = int.Parse(_config["Smtp:Port"]);
        //    var smtpUser = _config["Smtp:User"];
        //    var smtpPass = _config["Smtp:Pass"];

        //    var message = new MailMessage();
        //    message.To.Add(toEmail);
        //    message.Subject = subject;
        //    message.Body = body;
        //    message.IsBodyHtml = true;
        //    message.From = new MailAddress(smtpUser);

        //    using var client = new SmtpClient(smtpHost, smtpPort)
        //    {
        //        Credentials = new NetworkCredential(smtpUser, smtpPass),
        //        EnableSsl = true
        //    };

        //    await client.SendMailAsync(message);
        //}

        public async Task SendEmailAsync(string to, string subject, string htmlMessage)
        {
            var smtpSection = _config.GetSection("Smtp");
            using var client = new SmtpClient(smtpSection["Host"], int.Parse(smtpSection["Port"]))
            {
                Credentials = new NetworkCredential(smtpSection["User"], smtpSection["Pass"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage(smtpSection["User"], to, subject, htmlMessage)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(mailMessage);
        }
    }
}
