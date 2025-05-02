using PaymentProcessing.Domain.Entities;
namespace PaymentProcessing.Domain.Model
{
    public class PaymentProvider
    {
        public Guid ProviderId { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
    }
}
