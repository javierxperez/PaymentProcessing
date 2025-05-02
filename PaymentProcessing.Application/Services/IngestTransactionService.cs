
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentProcessing.Application.DTOs;
using PaymentProcessing.Domain.Entities;
using PaymentProcessing.Domain.Enums;
using PaymentProcessing.Domain.Interfaces;

namespace PaymentProcessing.Application.Services
{
    public class IngestTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<IngestTransactionService> _logger;
        private const int MaxRetries = 3;

        public IngestTransactionService(IUnitOfWork unitOfWork, ILogger<IngestTransactionService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<Result> IngestAsync(string providerName, IngestTransactionRequest request)
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    var provider = await _unitOfWork.PaymentProviders.GetByNameAsync(providerName);
                    if (provider == null)
                        return Result.Failure($"Unknown provider: {providerName}");

                    var method = await _unitOfWork.PaymentMethods.GetByNameAsync(request.PaymentMethod);
                    if (method == null)
                        return Result.Failure($"Unknown payment method: {request.PaymentMethod}");

                    if (!Enum.TryParse<PaymentStatusEnum>(request.Status, true, out var status))
                        return Result.Failure($"Invalid status: {request.Status}");

                    //if a transaction with the same parameters occurs withing a 5 minute window consider dupe
                    var timeWindowStart = DateTime.UtcNow.AddMinutes(-5);
                    var timeWindowEnd = DateTime.UtcNow.AddMinutes(5);

                    var isDuplicate = await _unitOfWork.PaymentTransactions.ExistsDuplicateAsync
                                        (provider.ProviderId, request.Amount, request.Currency, request.PayerEmail, timeWindowStart, timeWindowEnd);


                    if (isDuplicate)
                        return Result.Failure("Duplicate transaction detected.");

                    var transaction = new PaymentTransaction
                    {
                        TransactionId = Guid.NewGuid(),
                        Amount = request.Amount,
                        Currency = request.Currency,
                        Status = status,
                        TimestampUtc = DateTime.UtcNow,
                        PayerEmail = request.PayerEmail,
                        ProviderId = provider.ProviderId,
                        PaymentMethodId = method.PaymentMethodId
                    };

                    await _unitOfWork.PaymentTransactions.AddAsync(transaction);
                    await _unitOfWork.CommitAsync();

                    return Result.Success();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to ingest transaction (attempt {Attempt})", attempt);

                    if (attempt == MaxRetries)
                    {
                        return Result.Failure("An unexpected error occurred after multiple attempts: " + ex.Message);
                    }
                    await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt));
                }
            }
            return Result.Failure("Ingest failed after retrying.");
        }
    }
}
