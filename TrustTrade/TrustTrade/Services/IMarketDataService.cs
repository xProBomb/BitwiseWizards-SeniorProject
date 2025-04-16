using TrustTrade.ViewModels;

public interface IMarketDataService
{
    Task<StockViewModel?> GetStockQuoteAsync(string ticker);
}
