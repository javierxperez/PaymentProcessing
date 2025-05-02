namespace PaymentProcessing.Domain.Entities
{
    public class PaymentMethod
    {
        public Guid PaymentMethodId { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
    }
}
