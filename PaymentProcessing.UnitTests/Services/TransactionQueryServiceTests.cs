using Xunit;
using Moq;
using PaymentProcessing.Application.Services;
using PaymentProcessing.Application.DTOs;
using PaymentProcessing.Domain.Interfaces;
using PaymentProcessing.Domain.Entities;
using PaymentProcessing.Domain.Enums;
using PaymentProcessing.Domain.Interfaces.Repositories;
using PaymentProcessing.Domain.Model;
using Microsoft.Extensions.Logging;
using PaymentProcessing.UnitTests.Helper;
namespace PaymentProcessing.UnitTests.Services
{
    public class TransactionQueryServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<ILogger<TransactionQueryService>> _logger = new();
        private readonly Mock<IPaymentTransactionRepository> _transactionRepoMock = new();
        private readonly TransactionQueryService _service;

        public TransactionQueryServiceTests()
        {
            _unitOfWorkMock.SetupGet(u => u.PaymentTransactions).Returns(_transactionRepoMock.Object);
            _service = new TransactionQueryService(_unitOfWorkMock.Object, _logger.Object);
        }

        [Fact]
        public async Task GetTransactionsAsync_ShouldFilterByProvider()
        {
            var transactions = new List<PaymentTransaction>
        {
            new() { Provider = new PaymentProvider { Name = "PayPal" }, TimestampUtc = DateTime.UtcNow, PaymentMethod = new PaymentMethod() },
            new() { Provider = new PaymentProvider { Name = "Trustly" }, TimestampUtc = DateTime.UtcNow, PaymentMethod = new PaymentMethod() }
        }.AsQueryable();

            _transactionRepoMock.Setup(r => r.AsQueryable()).Returns(transactions);

            var result = await _service.GetTransactionsAsync(new TransactionFilterRequest { ProviderName = "PayPal" });

            Assert.All(result, r => Assert.Equal("PayPal", r.ProviderName));
        }

        [Fact]
        public async Task GetTransactionsAsync_ShouldFilterByStatus()
        {
            var transactions = new List<PaymentTransaction>
        {
            new() { Status = PaymentStatusEnum.Completed, TimestampUtc = DateTime.UtcNow, Provider = new PaymentProvider(), PaymentMethod = new PaymentMethod() },
            new() { Status = PaymentStatusEnum.Failed, TimestampUtc = DateTime.UtcNow, Provider = new PaymentProvider(), PaymentMethod = new PaymentMethod() }
        }.AsQueryable();

            _transactionRepoMock.Setup(r => r.AsQueryable()).Returns(transactions);

            var result = await _service.GetTransactionsAsync(new TransactionFilterRequest { Status = PaymentStatusEnum.Completed });

            Assert.All(result, r => Assert.Equal("Completed", r.Status));
        }
        [Fact]
        public async Task GetTransactionsAsync_ShouldReturnAllIfNoFilters()
        {
            var transactions = new List<PaymentTransaction>
            {
                new()
                {
                    TransactionId = Guid.NewGuid(),
                    Amount = 100,
                    Currency = "USD",
                    Status = PaymentStatusEnum.Completed,
                    TimestampUtc = DateTime.UtcNow,
                    PayerEmail = "test@example.com",
                    Provider = new PaymentProvider { Name = "PayPal" },
                    PaymentMethod = new PaymentMethod { Name = "CreditCard" }
                }
            }.AsQueryable();

            _transactionRepoMock.Setup(r => r.AsQueryable())
                .Returns(new TestAsyncEnumerable<PaymentTransaction>(transactions));

            var result = await _service.GetTransactionsAsync(new TransactionFilterRequest());

            Assert.Single(result);
            Assert.Equal("PayPal", result[0].ProviderName);
            Assert.Equal("CreditCard", result[0].PaymentMethod);
        }

        [Fact]
        public async Task GetTransactionsAsync_ShouldReturnEmptyList_WhenExceptionThrown()
        {
            _transactionRepoMock.Setup(r => r.AsQueryable())
                .Throws(new Exception("Simulated failure"));

            var result = await _service.GetTransactionsAsync(new TransactionFilterRequest());

            Assert.NotNull(result);
            Assert.Empty(result);
        }
        [Fact]
        public async Task GetTransactionsAsync_ShouldHandleMissingProviderOrMethodGracefully()
        {
            var transactions = new List<PaymentTransaction>
            {
                new() { TimestampUtc = DateTime.UtcNow, Provider = null!, PaymentMethod = null! }
            }.AsQueryable();

            _transactionRepoMock.Setup(r => r.AsQueryable()).Returns(transactions);

            var result = await _service.GetTransactionsAsync(new TransactionFilterRequest());

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSummaryAsync_ShouldReturnCorrectCount()
        {
            var transactions = new List<PaymentTransaction>
        {
            new() { Status = PaymentStatusEnum.Completed, Amount = 100, Provider = new PaymentProvider { Name = "PayPal" } },
            new() { Status = PaymentStatusEnum.Pending, Amount = 50, Provider = new PaymentProvider { Name = "PayPal" } }
        };

            _transactionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(transactions);

            var summary = await _service.GetSummaryAsync();

            Assert.Equal(2, summary?.TotalTransactions);
            Assert.Equal(150, summary?.VolumePerProvider["PayPal"]);
        }
        [Fact]
        public async Task GetSummaryAsync_ShouldReturnNull_WhenExceptionThrown()
        {
            _transactionRepoMock.Setup(r => r.GetAllAsync())
                .ThrowsAsync(new Exception("Simulated failure"));

            var summary = await _service.GetSummaryAsync();

            Assert.Null(summary);
        }
        [Fact]
        public async Task GetSummaryAsync_ShouldHandleEmptyData()
        {
            _transactionRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<PaymentTransaction>());

            var summary = await _service.GetSummaryAsync();

            Assert.NotNull(summary);
            Assert.Equal(0, summary.TotalTransactions);
            Assert.Empty(summary.VolumePerProvider);
            Assert.Empty(summary.StatusBreakdown);
        }
    }
}
