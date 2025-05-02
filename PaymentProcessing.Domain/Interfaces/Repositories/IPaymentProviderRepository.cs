using PaymentProcessing.Domain.Model;
namespace PaymentProcessing.Domain.Interfaces.Repositories
{
    public interface IPaymentProviderRepository
    {
        Task<PaymentProvider?> GetByNameAsync(string name);
    }
}
