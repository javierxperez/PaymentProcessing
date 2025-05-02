using PaymentProcessing.Domain.Enums;
namespace PaymentProcessing.Application.DTOs
{
    public class TransactionFilterRequest
    {
        public string? ProviderName { get; set; }
        public PaymentStatusEnum? Status { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
