namespace PaymentProcessing.Application.DTOs
{
    public class IngestTransactionRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public string PayerEmail { get; set; } = null!;
        public string PaymentMethod { get; set; } = null!;
        public string Status { get; set; } = null!;
    }
}
