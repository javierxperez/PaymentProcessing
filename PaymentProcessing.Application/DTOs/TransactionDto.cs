namespace PaymentProcessing.Application.DTOs
{
    public class TransactionDto
    {
        public Guid TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime TimestampUtc { get; set; }
        public string PayerEmail { get; set; } = null!;
        public string ProviderName { get; set; } = null!;
        public string PaymentMethod { get; set; } = null!;
    }
}
