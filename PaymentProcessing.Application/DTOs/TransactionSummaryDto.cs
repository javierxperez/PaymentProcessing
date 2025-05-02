namespace PaymentProcessing.Application.DTOs
{
    public class TransactionSummaryDto
    {
        public int TotalTransactions { get; set; }
        public Dictionary<string, decimal> VolumePerProvider { get; set; } = new();
        public Dictionary<string, int> StatusBreakdown { get; set; } = new();
    }
}
