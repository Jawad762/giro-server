using Giro.Api.Interfaces;
using System.Net.Mail;
using System.Net;

namespace Giro.Api.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        { 
            _configuration = configuration;
        }

        public async Task SendEmailConfirmationAsync(string toEmail, int code)
        {
            var smtpHost = _configuration["SmtpSettings:Host"];
            var smtpPort = int.Parse(_configuration["SmtpSettings:Port"]);
            var smtpEnableSsl = bool.Parse(_configuration["SmtpSettings:EnableSsl"]);
            var smtpUseDefaultCredentials = bool.Parse(_configuration["SmtpSettings:UseDefaultCredentials"]);
            var smtpUsername = _configuration["SmtpSettings:Username"];
            var smtpPassword = _configuration["SmtpSettings:Password"];

            var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = smtpEnableSsl,
                UseDefaultCredentials = smtpUseDefaultCredentials,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            var fromAddress = new MailAddress(smtpUsername, "Giro");
            var toAddress = new MailAddress(toEmail);

            string body = $@"
    <!DOCTYPE html>
    <html lang='en'>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <title>Email Verification</title>
        <style>
            body {{
                font-family: Arial, sans-serif;
                background-color: #f4f4f4;
                margin: 0;
                padding: 20px;
            }}
            .container {{
                max-width: 600px;
                margin: 0 auto;
                background: #ffffff;
                padding: 20px;
                border-radius: 5px;
                box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
            }}
            h1 {{
                color: #333333;
            }}
            .code {{
                font-size: 24px;
                font-weight: bold;
                color: #007BFF;
                margin: 20px 0;
            }}
            p {{
                color: #555555;
                line-height: 1.5;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <h1>Welcome to Giro!</h1>
            <p>Thank you for signing up. Please use the verification code below to complete your registration:</p>
            <div class='code'>{code}</div>
            <p>If you did not sign up for this account, please ignore this email.</p>
            <p>Best regards</p>
        </div>
    </body>
    </html>";

            var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = toEmail,
                Body = body,
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
        }
    }
}
