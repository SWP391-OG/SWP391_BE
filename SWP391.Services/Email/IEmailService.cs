namespace SWP391.Services.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendVerificationEmailAsync(string toEmail, string verificationCode);
        Task SendPasswordResetEmailAsync(string toEmail, string resetCode);
    }
}