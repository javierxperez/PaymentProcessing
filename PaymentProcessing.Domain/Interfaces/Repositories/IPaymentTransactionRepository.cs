using PaymentProcessing.Domain.Entities;
namespace PaymentProcessing.Domain.Interfaces.Repositories
{
    public interface IPaymentTransactionRepository
    {
        Task AddAsync(PaymentTransaction transaction);
        IQueryable<PaymentTransaction> AsQueryable();
        Task<List<PaymentTransaction>> GetAllAsync();
        Task<bool> ExistsDuplicateAsync(Guid providerId, decimal amount, string currency, string email, DateTime start, DateTime end);
    }
}
