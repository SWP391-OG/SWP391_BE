using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace SWP391.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpHost = _configuration["Email:Smtp:Host"];
            var smtpPort = int.Parse(_configuration["Email:Smtp:Port"]);
            var smtpUsername = _configuration["Email:Smtp:Username"];
            var smtpPassword = _configuration["Email:Smtp:Password"];
            var smtpFrom = _configuration["Email:Smtp:From"];

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpFrom),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }

        public async Task SendVerificationEmailAsync(string toEmail, string verificationCode)
        {
            var subject = "Email Verification - FPTechnical";
            var body = $@"
                <h2>Email Verification</h2>
                <p>Thank you for registering with FPTechnical.</p>
                <p>Your verification code is: <strong>{verificationCode}</strong></p>
                <p>This code will expire in 15 minutes.</p>
                <p>If you didn't request this, please ignore this email.</p>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetCode)
        {
            var subject = "Password Reset - FPTechnical";
            var body = $@"
                <h2>Password Reset Request</h2>
                <p>You have requested to reset your password.</p>
                <p>Your reset code is: <strong>{resetCode}</strong></p>
                <p>This code will expire in 15 minutes.</p>
                <p>If you didn't request this, please ignore this email.</p>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}