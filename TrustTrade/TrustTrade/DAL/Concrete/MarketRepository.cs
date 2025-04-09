using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrustTrade.DAL.Abstract;
using TrustTrade.ViewModels;

public class MockMarketRepository : IMarketRepository
{
    private readonly List<StockViewModel> _mockData;

    public MockMarketRepository()
    {
        _mockData = new List<StockViewModel>
        {
            new StockViewModel { Ticker = "AAPL", Name = "Apple Inc.", Price = 187.92m, Change = +2.35m },
            new StockViewModel { Ticker = "GOOGL", Name = "Alphabet Inc.", Price = 2743.57m, Change = -12.41m },
            new StockViewModel { Ticker = "TSLA", Name = "Tesla Inc.", Price = 791.36m, Change = +15.20m },
            new StockViewModel { Ticker = "BTC", Name = "Bitcoin", Price = 62205.43m, Change = -432.12m },
            new StockViewModel { Ticker = "ETH", Name = "Ethereum", Price = 2940.20m, Change = +56.70m },
            new StockViewModel { Ticker = "MSFT", Name = "Microsoft Corp", Price = 323.17m, Change = +4.02m },
            // ...add more mock data as needed
        };
    }

    public Task<List<StockViewModel>> SearchStocksAsync(string searchTerm, bool isCrypto)
    {
        var results = _mockData
            .Where(s => (s.Ticker.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                      || s.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                     && (isCrypto ? IsCrypto(s.Ticker) : !IsCrypto(s.Ticker)))
            .Take(12)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<List<StockViewModel>> GetTopMarketAsync(bool isCrypto)
    {
        var results = _mockData
            .Where(s => isCrypto ? IsCrypto(s.Ticker) : !IsCrypto(s.Ticker))
            .Take(6)
            .ToList();

        return Task.FromResult(results);
    }

    private bool IsCrypto(string ticker)
    {
        var cryptoTickers = new[] { "BTC", "ETH", "DOGE", "SOL", "ADA" };
        return cryptoTickers.Contains(ticker.ToUpper());
    }
}
