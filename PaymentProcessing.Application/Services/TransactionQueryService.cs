using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentProcessing.Application.DTOs;
using PaymentProcessing.Domain.Interfaces;

namespace PaymentProcessing.Application.Services
{
    public class TransactionQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransactionQueryService> _logger;

        public TransactionQueryService(IUnitOfWork unitOfWork, ILogger<TransactionQueryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<List<TransactionDto>> GetTransactionsAsync(TransactionFilterRequest filter)
        {
            try
            {
                var query = _unitOfWork.PaymentTransactions.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter.ProviderName))
                    query = query.Where(t => t.Provider.Name == filter.ProviderName);

                if (filter.Status.HasValue)
                    query = query.Where(t => t.Status == filter.Status.Value);

                if (filter.From.HasValue)
                    query = query.Where(t => t.TimestampUtc >= filter.From.Value);

                if (filter.To.HasValue)
                    query = query.Where(t => t.TimestampUtc <= filter.To.Value);

                return await query
                    .Select(t => new TransactionDto
                    {
                        TransactionId = t.TransactionId,
                        Amount = t.Amount,
                        Currency = t.Currency,
                        Status = t.Status.ToString(),
                        TimestampUtc = t.TimestampUtc,
                        ProviderName = t.Provider.Name,
                        PaymentMethod = t.PaymentMethod.Name,
                        PayerEmail = t.PayerEmail
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch transactions with filters: {@Filter}", filter);
                return new List<TransactionDto>();
            }
        }
        public async Task<TransactionSummaryDto?> GetSummaryAsync()
        {
            try
            {
                var transactions = await _unitOfWork.PaymentTransactions.GetAllAsync();

                var total = transactions.Count;

                var volumePerProvider = transactions
                    .GroupBy(t => t.Provider.Name)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

                var statusBreakdown = transactions
                    .GroupBy(t => t.Status)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count());

                return new TransactionSummaryDto
                {
                    TotalTransactions = total,
                    VolumePerProvider = volumePerProvider,
                    StatusBreakdown = statusBreakdown
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate transaction summary.");
                return default; 
            }
        }
    }
}
