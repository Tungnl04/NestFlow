namespace NestFlow.Application.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendVerificationCodeAsync(string toEmail, string verificationCode, string userName);
        Task SendWelcomeEmailAsync(string toEmail, string userName);
    }
}
