using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrustTrade.Models;
using TrustTrade.ViewModels;

namespace TrustTrade.Controllers
{
    public class MarketController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(string type = "stock")
        {
            ViewBag.Type = type;

            // Simulate async behavior for now....
            var data = await Task.FromResult(GetSampleData(type));

            // Return partial if AJAX
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_MarketCardsPartial", data);
            }

            return View("MarketHome", data);
        }

        // Mock data function
        private List<StockViewModel> GetSampleData(string type)
        {
            if (type == "crypto")
            {
                return new List<StockViewModel>
                {
                    new StockViewModel { Ticker = "BTC", Name = "Bitcoin", Price = 64123.45m, Change = 2.13m },
                    new StockViewModel { Ticker = "ETH", Name = "Ethereum", Price = 3275.22m, Change = -0.87m },
                    new StockViewModel { Ticker = "SOL", Name = "Solana", Price = 112.34m, Change = 1.92m },
                    new StockViewModel { Ticker = "DOGE", Name = "Dogecoin", Price = 0.128m, Change = 3.87m },
                    new StockViewModel { Ticker = "ADA", Name = "Cardano", Price = 0.654m, Change = -1.34m },
                    new StockViewModel { Ticker = "BNB", Name = "Binance Coin", Price = 389.75m, Change = 0.58m },
                };
            }

            return new List<StockViewModel>
            {
                new StockViewModel { Ticker = "AAPL", Name = "Apple Inc.", Price = 175.32m, Change = 1.56m },
                new StockViewModel { Ticker = "MSFT", Name = "Microsoft Corp", Price = 295.65m, Change = -0.87m },
                new StockViewModel { Ticker = "TSLA", Name = "Tesla Inc", Price = 220.21m, Change = 2.12m },
                new StockViewModel { Ticker = "NVDA", Name = "NVIDIA Corp", Price = 498.76m, Change = -4.23m },
                new StockViewModel { Ticker = "AMZN", Name = "Amazon.com Inc", Price = 132.45m, Change = 0.62m },
                new StockViewModel { Ticker = "GOOGL", Name = "Alphabet Inc", Price = 138.90m, Change = 1.02m },
            };
        }
    }
}

namespace TrustTrade.Controllers
{
    [Route("api/market")]
    public class MarketApiController : Controller
    {
        [HttpGet("history")]
        public IActionResult GetMockHistory(string ticker)
        {
            var random = new Random();
            var data = new List<object>();
            decimal basePrice = 150.00m;

            for (int i = 0; i < 20; i++)
            {
                basePrice += (decimal)(random.NextDouble() * 3 - 1.5); // small random walk for now.....
                data.Add(new
                {
                    time = $"10:{(i * 3):D2}",
                    price = Math.Round(basePrice, 2)
                });
            }

            return Json(data);
        }
    }
}
