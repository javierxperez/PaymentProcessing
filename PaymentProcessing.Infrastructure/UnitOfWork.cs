using PaymentProcessing.Domain.Interfaces;
using PaymentProcessing.Domain.Interfaces.Repositories;
using PaymentProcessing.Infrastructure.Data;
namespace PaymentProcessing.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EFDBContext _context;

        public UnitOfWork(EFDBContext context,
                          IPaymentTransactionRepository transactions,
                          IPaymentProviderRepository providers,
                          IPaymentMethodRepository methods)
        {
            _context = context;
            PaymentTransactions = transactions;
            PaymentProviders = providers;
            PaymentMethods = methods;
        }

        public IPaymentTransactionRepository PaymentTransactions { get; }
        public IPaymentProviderRepository PaymentProviders { get; }
        public IPaymentMethodRepository PaymentMethods { get; }

        public Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
