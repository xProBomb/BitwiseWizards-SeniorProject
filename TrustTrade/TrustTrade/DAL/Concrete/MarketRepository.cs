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
            // 🔹 Stocks
            new StockViewModel { Ticker = "AAPL", Name = "Apple Inc.", Price = 187.92m, Change = +2.35m },
            new StockViewModel { Ticker = "GOOGL", Name = "Alphabet Inc.", Price = 2743.57m, Change = -12.41m },
            new StockViewModel { Ticker = "TSLA", Name = "Tesla Inc.", Price = 791.36m, Change = +15.20m },
            new StockViewModel { Ticker = "MSFT", Name = "Microsoft Corp", Price = 323.17m, Change = +4.02m },
            new StockViewModel { Ticker = "AMZN", Name = "Amazon.com Inc.", Price = 3441.85m, Change = -3.40m },
            new StockViewModel { Ticker = "NFLX", Name = "Netflix Inc.", Price = 645.15m, Change = +1.12m },
            new StockViewModel { Ticker = "NVDA", Name = "NVIDIA Corp", Price = 891.20m, Change = +5.50m },
            new StockViewModel { Ticker = "META", Name = "Meta Platforms Inc.", Price = 364.70m, Change = -1.32m },
            new StockViewModel { Ticker = "BABA", Name = "Alibaba Group", Price = 104.58m, Change = +2.14m },
            new StockViewModel { Ticker = "DIS", Name = "The Walt Disney Company", Price = 112.34m, Change = +0.77m },
            new StockViewModel { Ticker = "UBER", Name = "Uber Technologies Inc.", Price = 62.15m, Change = +1.09m },
            new StockViewModel { Ticker = "PYPL", Name = "PayPal Holdings Inc.", Price = 71.34m, Change = -0.65m },

            // 🔸 Crypto
            new StockViewModel { Ticker = "BTC", Name = "Bitcoin", Price = 62205.43m, Change = -432.12m },
            new StockViewModel { Ticker = "ETH", Name = "Ethereum", Price = 2940.20m, Change = +56.70m },
            new StockViewModel { Ticker = "SOL", Name = "Solana", Price = 151.02m, Change = +4.90m },
            new StockViewModel { Ticker = "ADA", Name = "Cardano", Price = 0.64m, Change = -0.05m },
            new StockViewModel { Ticker = "DOGE", Name = "Dogecoin", Price = 0.16m, Change = +0.01m },
            new StockViewModel { Ticker = "XRP", Name = "Ripple", Price = 0.56m, Change = -0.02m },
            new StockViewModel { Ticker = "AVAX", Name = "Avalanche", Price = 36.78m, Change = +1.15m },
            new StockViewModel { Ticker = "MATIC", Name = "Polygon", Price = 0.85m, Change = -0.03m },
            new StockViewModel { Ticker = "LTC", Name = "Litecoin", Price = 91.21m, Change = +2.35m },
            new StockViewModel { Ticker = "BNB", Name = "Binance Coin", Price = 589.74m, Change = +12.64m }
        };
    }

    public Task<List<StockViewModel>> SearchStocksAsync(string searchTerm, bool isCrypto)
    {
        var results = _mockData
            .Where(s => (s.Ticker.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                      || s.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                     && (isCrypto ? IsCrypto(s.Ticker) : !IsCrypto(s.Ticker)))
            .Take(20)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<List<StockViewModel>> GetTopMarketAsync(bool isCrypto)
    {
        var results = _mockData
            .Where(s => isCrypto ? IsCrypto(s.Ticker) : !IsCrypto(s.Ticker))
            .OrderByDescending(s => Math.Abs(s.Change))
            .Take(6)
            .ToList();

        return Task.FromResult(results);
    }

    private bool IsCrypto(string ticker)
    {
        var cryptoTickers = new[]
        {
            "BTC", "ETH", "DOGE", "SOL", "ADA", "XRP", "AVAX", "MATIC", "LTC", "BNB"
        };
        return cryptoTickers.Contains(ticker.ToUpper());
    }
}
