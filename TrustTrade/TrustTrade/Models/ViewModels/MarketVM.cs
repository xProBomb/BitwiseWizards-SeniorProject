namespace TrustTrade.ViewModels
{
    public class StockViewModel
    {
        public string Ticker { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal Change { get; set; }
        public decimal PerformanceScore { get; set; }
        public DateTime? LastUpdated { get; set; }

        public List<decimal> Highs { get; set; } = new();
    }
}
