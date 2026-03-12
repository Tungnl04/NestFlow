namespace NestFlow.Application.Constants
{
    /// <summary>
    /// Cấu hình chung của nền tảng NestFlow
    /// </summary>
    public static class PlatformSettings
    {
        /// <summary>
        /// Tỷ lệ giảm giá cho user khi đặt cọc qua nền tảng (%)
        /// Ví dụ: 50 = giảm 50%, 10 = giảm 10%
        /// </summary>
        public const decimal UserDiscountRate = 10.00m;

        /// <summary>
        /// Tỷ lệ hoa hồng nền tảng thu từ landlord (%)
        /// Ví dụ: 50 = thu 50%, 30 = thu 30%
        /// </summary>
        public const decimal PlatformCommissionRate = 20.00m;

        /// <summary>
        /// Tên hiển thị của discount (dùng cho UI)
        /// </summary>
        public static string DiscountDisplayText => $"Giảm {UserDiscountRate}%";

        /// <summary>
        /// Mô tả chi tiết về ưu đãi
        /// </summary>
        public static string DiscountDescription => $"Giảm ngay {UserDiscountRate}% tiền đặt cọc khi đặt qua NestFlow";
    }
}
