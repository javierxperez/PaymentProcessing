using Microsoft.EntityFrameworkCore;
using PaymentProcessing.Domain.Entities;
using PaymentProcessing.Domain.Interfaces.Repositories;
using PaymentProcessing.Infrastructure.Data;
namespace PaymentProcessing.Infrastructure.Repositories
{
    public class PaymentTransactionRepository : IPaymentTransactionRepository
    {
        private readonly EFDBContext _context;

        public PaymentTransactionRepository(EFDBContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PaymentTransaction transaction)
        {
            await _context.PaymentTransactions.AddAsync(transaction);
        }

        public IQueryable<PaymentTransaction> AsQueryable()
        {
            return _context.PaymentTransactions
                .Include(t => t.Provider)
                .Include(t => t.PaymentMethod);
        }
        public Task<bool> ExistsDuplicateAsync(Guid providerId, decimal amount, string currency, string email, DateTime start, DateTime end)
        {
            return _context.PaymentTransactions.AnyAsync(t =>
                t.ProviderId == providerId &&
                t.Amount == amount &&
                t.Currency == currency &&
                t.PayerEmail == email &&
                t.TimestampUtc >= start &&
                t.TimestampUtc <= end);
        }
        public async Task<List<PaymentTransaction>> GetAllAsync() => await AsQueryable().ToListAsync();
    }
}
