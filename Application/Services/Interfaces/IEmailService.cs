namespace NestFlow.Application.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendVerificationCodeAsync(string toEmail, string verificationCode, string userName);
        Task SendWelcomeEmailAsync(string toEmail, string userName);
        Task SendPaymentSuccessEmailAsync(string toEmail, string customerName, string propertyTitle, decimal amount, string orderCode);
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
    }
}
