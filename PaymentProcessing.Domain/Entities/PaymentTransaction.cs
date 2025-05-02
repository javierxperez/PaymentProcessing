using PaymentProcessing.Domain.Enums;
using PaymentProcessing.Domain.Model;
namespace PaymentProcessing.Domain.Entities
{
    public class PaymentTransaction
    {
        public Guid TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public PaymentStatusEnum Status { get; set; }
        public DateTime TimestampUtc { get; set; }
        public string PayerEmail { get; set; } = null!;

        public Guid PaymentMethodId { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = null!;

        public Guid ProviderId { get; set; }
        public PaymentProvider Provider { get; set; } = null!;
    }
}
