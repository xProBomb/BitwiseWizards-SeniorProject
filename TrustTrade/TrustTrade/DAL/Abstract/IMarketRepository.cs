using TrustTrade.ViewModels;


public interface IMarketRepository
{
    Task<List<StockViewModel>> SearchStocksAsync(string searchTerm, bool isCrypto);
    Task<List<StockViewModel>> GetTopMarketAsync(bool isCrypto);

    Task<List<(DateTime Date, decimal High, decimal Low)>> GetHighLowHistoryAsync(string ticker, int days = 30);

    
}

