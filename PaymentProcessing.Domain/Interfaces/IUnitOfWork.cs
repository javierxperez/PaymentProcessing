using PaymentProcessing.Domain.Interfaces.Repositories;
namespace PaymentProcessing.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IPaymentTransactionRepository PaymentTransactions { get; }
        IPaymentProviderRepository PaymentProviders { get; }
        IPaymentMethodRepository PaymentMethods { get; }

        Task<int> CommitAsync(CancellationToken cancellationToken = default);
    }
}
