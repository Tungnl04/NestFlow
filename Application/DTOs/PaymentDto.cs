namespace NestFlow.Application.DTOs
{
    public class CreatePaymentDto
    {
        public long PropertyId { get; set; }
        public DateTime BookingDate { get; set; }
        public string? Notes { get; set; }
    }

    public class PaymentResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? CheckoutUrl { get; set; }
        public long? BookingId { get; set; }
        public long? PaymentId { get; set; }
    }

    public class PaymentStatusDto
    {
        public long PaymentId { get; set; }
        public string Status { get; set; } = null!;
        public decimal Amount { get; set; }
        public string PaymentType { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
