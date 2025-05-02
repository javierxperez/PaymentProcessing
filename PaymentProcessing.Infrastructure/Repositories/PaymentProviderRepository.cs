using Microsoft.EntityFrameworkCore;
using PaymentProcessing.Domain.Interfaces.Repositories;
using PaymentProcessing.Domain.Model;
using PaymentProcessing.Infrastructure.Data;

namespace PaymentProcessing.Infrastructure.Repositories
{
    public class PaymentProviderRepository : IPaymentProviderRepository
    {
        private readonly EFDBContext _context;

        public PaymentProviderRepository(EFDBContext context)
        {
            _context = context;
        }

        public Task<PaymentProvider?> GetByNameAsync(string name)
        {
            return _context.PaymentProviders.FirstOrDefaultAsync(p => p.Name == name);
        }
    }
}
