using PaymentProcessing.Domain.Entities;
namespace PaymentProcessing.Domain.Interfaces.Repositories
{
    public interface IPaymentMethodRepository
    {
        Task<PaymentMethod?> GetByNameAsync(string name);
    }
}
