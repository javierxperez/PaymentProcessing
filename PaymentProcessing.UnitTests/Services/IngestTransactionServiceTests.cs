using Xunit;
using Moq;
using PaymentProcessing.Application.Services;
using PaymentProcessing.Application.DTOs;
using PaymentProcessing.Domain.Interfaces;
using PaymentProcessing.Domain.Entities;
using PaymentProcessing.Domain.Interfaces.Repositories;
using PaymentProcessing.Domain.Model;
using Microsoft.Extensions.Logging;
namespace PaymentProcessing.UnitTests.Services
{
    public class IngestTransactionServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<ILogger<IngestTransactionService>> _logger = new();
        private readonly Mock<IPaymentProviderRepository> _providerRepoMock = new();
        private readonly Mock<IPaymentMethodRepository> _methodRepoMock = new();
        private readonly Mock<IPaymentTransactionRepository> _transactionRepoMock = new();
        private readonly IngestTransactionService _service;

        public IngestTransactionServiceTests()
        {
            _unitOfWorkMock.SetupGet(u => u.PaymentProviders).Returns(_providerRepoMock.Object);
            _unitOfWorkMock.SetupGet(u => u.PaymentMethods).Returns(_methodRepoMock.Object);
            _unitOfWorkMock.SetupGet(u => u.PaymentTransactions).Returns(_transactionRepoMock.Object);
            _service = new IngestTransactionService(_unitOfWorkMock.Object, _logger.Object);
        }

        [Fact]
        public async Task IngestAsync_ShouldReturnFailure_WhenProviderIsInvalid()
        {
            _providerRepoMock.Setup(r => r.GetByNameAsync("Invalid")).ReturnsAsync((PaymentProvider?)null);

            var result = await _service.IngestAsync("Invalid", new IngestTransactionRequest());

            Assert.True(result.IsFailure);
            Assert.Contains("Unknown provider", result.Error);
        }

        [Fact]
        public async Task IngestAsync_ShouldReturnFailure_WhenPaymentMethodIsInvalid()
        {
            _providerRepoMock.Setup(r => r.GetByNameAsync("PayPal")).ReturnsAsync(new PaymentProvider { ProviderId = Guid.NewGuid(), Name = "PayPal" });
            _methodRepoMock.Setup(r => r.GetByNameAsync("Invalid")).ReturnsAsync((PaymentMethod?)null);

            var result = await _service.IngestAsync("PayPal", new IngestTransactionRequest { PaymentMethod = "Invalid" });

            Assert.True(result.IsFailure);
            Assert.Contains("Unknown payment method", result.Error);
        }

        [Fact]
        public async Task IngestAsync_ShouldReturnFailure_WhenStatusIsInvalid()
        {
            _providerRepoMock.Setup(r => r.GetByNameAsync("PayPal")).ReturnsAsync(new PaymentProvider { ProviderId = Guid.NewGuid(), Name = "PayPal" });
            _methodRepoMock.Setup(r => r.GetByNameAsync("ACH")).ReturnsAsync(new PaymentMethod { PaymentMethodId = Guid.NewGuid(), Name = "ACH" });

            var result = await _service.IngestAsync("PayPal", new IngestTransactionRequest { PaymentMethod = "ACH", Status = "BadStatus" });

            Assert.True(result.IsFailure);
            Assert.Contains("Invalid status", result.Error);
        }

        [Fact]
        public async Task IngestAsync_ShouldReturnSuccess_WhenAllValid()
        {
            var provider = new PaymentProvider { ProviderId = Guid.NewGuid(), Name = "PayPal" };
            var method = new PaymentMethod { PaymentMethodId = Guid.NewGuid(), Name = "ACH" };

            _providerRepoMock.Setup(r => r.GetByNameAsync("PayPal")).ReturnsAsync(provider);
            _methodRepoMock.Setup(r => r.GetByNameAsync("ACH")).ReturnsAsync(method);

            var request = new IngestTransactionRequest
            {
                Amount = 100,
                Currency = "USD",
                Status = "Completed",
                PayerEmail = "payer@example.com",
                PaymentMethod = "ACH"
            };

            _transactionRepoMock
                .Setup(r => r.ExistsDuplicateAsync(
                    provider.ProviderId,
                    request.Amount,
                    request.Currency,
                    request.PayerEmail,
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()
                ))
                .ReturnsAsync(false);

            var result = await _service.IngestAsync("PayPal", request);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task IngestAsync_ShouldReturnFailure_WhenRepositoryThrowsException()
        {
            _providerRepoMock.Setup(r => r.GetByNameAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Simulated DB failure"));

            var request = new IngestTransactionRequest
            {
                Amount = 100,
                Currency = "USD",
                Status = "Completed",
                PayerEmail = "payer@example.com",
                PaymentMethod = "ACH"
            };

            var result = await _service.IngestAsync("PayPal", request);

            Assert.True(result.IsFailure);
            Assert.Contains("An unexpected error occurred", result.Error);
        }
        [Fact]
        public async Task IngestAsync_ShouldReturnFailure_WhenDuplicateExists()
        {
            var provider = new PaymentProvider { ProviderId = Guid.NewGuid(), Name = "PayPal" };
            var method = new PaymentMethod { PaymentMethodId = Guid.NewGuid(), Name = "ACH" };

            _providerRepoMock.Setup(r => r.GetByNameAsync("PayPal")).ReturnsAsync(provider);
            _methodRepoMock.Setup(r => r.GetByNameAsync("ACH")).ReturnsAsync(method);

            _transactionRepoMock.Setup(r => r.ExistsDuplicateAsync(
                provider.ProviderId,
                100,
                "USD",
                "payer@example.com",
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()
            )).ReturnsAsync(true);

            var request = new IngestTransactionRequest
            {
                Amount = 100,
                Currency = "USD",
                Status = "Completed",
                PayerEmail = "payer@example.com",
                PaymentMethod = "ACH"
            };

            var result = await _service.IngestAsync("PayPal", request);

            Assert.True(result.IsFailure);
            Assert.Contains("Duplicate transaction detected", result.Error ?? string.Empty);
        }
        [Fact]
        public async Task IngestAsync_ShouldReturnFailure_WhenCommitThrows()
        {
            var provider = new PaymentProvider { ProviderId = Guid.NewGuid(), Name = "PayPal" };
            var method = new PaymentMethod { PaymentMethodId = Guid.NewGuid(), Name = "ACH" };

            _providerRepoMock.Setup(r => r.GetByNameAsync("PayPal")).ReturnsAsync(provider);
            _methodRepoMock.Setup(r => r.GetByNameAsync("ACH")).ReturnsAsync(method);
            _transactionRepoMock.Setup(r => r.AsQueryable()).Returns(Enumerable.Empty<PaymentTransaction>().AsQueryable());
            _unitOfWorkMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Simulated commit failure"));

            var request = new IngestTransactionRequest
            {
                Amount = 100,
                Currency = "USD",
                Status = "Completed",
                PayerEmail = "payer@example.com",
                PaymentMethod = "ACH"
            };

            var result = await _service.IngestAsync("PayPal", request);

            Assert.True(result.IsFailure);
            Assert.Contains("An unexpected error occurred", result.Error);
        }
        [Fact]
        public async Task IngestAsync_ShouldReturnFailure_WhenRequiredFieldsAreMissing()
        {
            var provider = new PaymentProvider { ProviderId = Guid.NewGuid(), Name = "PayPal" };
            var method = new PaymentMethod { PaymentMethodId = Guid.NewGuid(), Name = "ACH" };

            _providerRepoMock.Setup(r => r.GetByNameAsync("PayPal")).ReturnsAsync(provider);
            _methodRepoMock.Setup(r => r.GetByNameAsync("ACH")).ReturnsAsync(method);

            _transactionRepoMock.Setup(r => r.ExistsDuplicateAsync(
                It.IsAny<Guid>(),
                It.IsAny<decimal>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()
            )).ReturnsAsync(false); 

            var request = new IngestTransactionRequest
            {
                Amount = 0,                  
                Currency = "",               
                Status = "Completed",        
                PayerEmail = "",             
                PaymentMethod = ""       
            };

            var result = await _service.IngestAsync("PayPal", request);

            Assert.True(result.IsFailure);
        }
    }
}