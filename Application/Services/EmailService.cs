using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using NestFlow.Application.Services.Interfaces;

namespace NestFlow.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _fromEmail = _configuration["EmailSettings:FromEmail"];
            _fromName = _configuration["EmailSettings:FromName"];
            _smtpHost = _configuration["EmailSettings:SmtpHost"];
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            _smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            _smtpPassword = _configuration["EmailSettings:SmtpPassword"];
        }

        public async Task SendVerificationCodeAsync(string toEmail, string verificationCode, string userName)
        {
            var subject = "Mã xác thực đặt lại mật khẩu - NestFlow";

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .code-box {{ background: white; border: 2px dashed #667eea; padding: 20px; text-align: center; margin: 20px 0; border-radius: 5px; }}
        .code {{ font-size: 32px; font-weight: bold; color: #667eea; letter-spacing: 5px; }}
        .warning {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🏠 NestFlow</h1>
            <p>Đặt lại mật khẩu</p>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{userName}</strong>,</p>
            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. Vui lòng sử dụng mã xác thực bên dưới:</p>
            
            <div class='code-box'>
                <div class='code'>{verificationCode}</div>
            </div>

            <div class='warning'>
                <strong>⚠️ Lưu ý:</strong>
                <ul style='margin: 10px 0 0 0; padding-left: 20px;'>
                    <li>Mã xác thực có hiệu lực trong <strong>15 phút</strong></li>
                    <li>Không chia sẻ mã này với bất kỳ ai</li>
                    <li>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này</li>
                </ul>
            </div>

            <p>Trân trọng,<br>Đội ngũ NestFlow</p>
        </div>
        <div class='footer'>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
            <p>&copy; 2024 NestFlow. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var subject = "Chào mừng bạn đến với NestFlow!";

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; padding: 12px 30px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🏠 NestFlow</h1>
            <p>Chào mừng bạn!</p>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{userName}</strong>,</p>
            <p>Chúc mừng bạn đã đăng ký thành công tài khoản NestFlow!</p>
            <p>Bạn có thể bắt đầu tìm kiếm căn hộ lý tưởng của mình ngay bây giờ.</p>
            <p style='text-align: center;'>
                <a href='https://nestflow.com' class='button'>Khám phá ngay</a>
            </p>
            <p>Trân trọng,<br>Đội ngũ NestFlow</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 NestFlow. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task SendPaymentSuccessEmailAsync(string toEmail, string customerName, string propertyTitle, decimal amount, string orderCode)
        {
            var subject = "Xác nhận thanh toán thành công - NestFlow";

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .info-box {{ background: white; padding: 20px; margin: 20px 0; border-left: 4px solid #667eea; border-radius: 5px; }}
        .amount {{ color: #667eea; font-size: 28px; font-weight: bold; }}
        .success-icon {{ color: #28a745; font-size: 48px; text-align: center; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🏠 NestFlow</h1>
            <p>Thanh toán thành công</p>
        </div>
        <div class='content'>
            <div class='success-icon'>✓</div>
            <p>Xin chào <strong>{customerName}</strong>,</p>
            <p>Cảm ơn bạn đã sử dụng dịch vụ của NestFlow. Giao dịch thanh toán của bạn đã được xử lý thành công.</p>
            
            <div class='info-box'>
                <h3 style='color: #667eea; margin-top: 0;'>Thông tin giao dịch</h3>
                <p><strong>Mã giao dịch:</strong> {orderCode}</p>
                <p><strong>Bất động sản:</strong> {propertyTitle}</p>
                <p><strong>Số tiền đặt cọc:</strong> <span class='amount'>{amount:N0} VNĐ</span></p>
                <p><strong>Thời gian:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
            </div>

            <p>Chủ nhà sẽ liên hệ với bạn trong thời gian sớm nhất để xác nhận lịch xem phòng.</p>
            
            <p>Nếu bạn có bất kỳ thắc mắc nào, vui lòng liên hệ:</p>
            <ul>
                <li>Hotline: 1900-xxxx</li>
                <li>Email: support@nestflow.com</li>
            </ul>

            <p>Trân trọng,<br>Đội ngũ NestFlow</p>
        </div>
        <div class='footer'>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
            <p>&copy; 2024 NestFlow. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_fromName, _fromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw;
            }
        }
    }
}
