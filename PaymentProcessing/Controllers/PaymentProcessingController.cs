using Microsoft.AspNetCore.Mvc;
using PaymentProcessing.Application.DTOs;
using PaymentProcessing.Application.Services;

namespace PaymentProcessing.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentProcessingController : ControllerBase
    {
        private readonly IngestTransactionService _ingestService;
        private readonly TransactionQueryService _queryService;

        public PaymentProcessingController(
            IngestTransactionService ingestService,
            TransactionQueryService queryService)
        {
            _ingestService = ingestService;
            _queryService = queryService;
        }

        [HttpPost("ingest/{providerName}")]
        public async Task<IActionResult> Ingest(string providerName, [FromBody] IngestTransactionRequest request)
        {
            var result = await _ingestService.IngestAsync(providerName, request);
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(new { message = "Transaction ingested successfully." });
        }
        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions([FromQuery] TransactionFilterRequest filter)
        {
            var transactions = await _queryService.GetTransactionsAsync(filter);
            return Ok(transactions);
        }
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _queryService.GetSummaryAsync();
            return Ok(summary);
        }
    }
}
