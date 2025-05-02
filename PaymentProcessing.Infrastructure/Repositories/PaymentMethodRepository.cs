using Microsoft.EntityFrameworkCore;
using PaymentProcessing.Domain.Entities;
using PaymentProcessing.Domain.Interfaces.Repositories;
using PaymentProcessing.Infrastructure.Data;
namespace PaymentProcessing.Infrastructure.Repositories
{
    public class PaymentMethodRepository : IPaymentMethodRepository
    {
        private readonly EFDBContext _context;

        public PaymentMethodRepository(EFDBContext context)
        {
            _context = context;
        }

        public Task<PaymentMethod?> GetByNameAsync(string name)
        {
            return _context.PaymentMethods.FirstOrDefaultAsync(m => m.Name == name);
        }
    }
}
