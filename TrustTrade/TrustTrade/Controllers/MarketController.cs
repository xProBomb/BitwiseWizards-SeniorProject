using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using TrustTrade.ViewModels;




public class MarketController : Controller
{
    private readonly IMarketRepository _marketRepo;
    private readonly IPerformanceScoreRepository _performanceScoreRepository;


    public MarketController(IMarketRepository marketRepo, IPerformanceScoreRepository performanceScoreRepository)
    {
        _performanceScoreRepository = performanceScoreRepository;
        _marketRepo = marketRepo; 
    }

    public async Task<IActionResult> Index(string type)
    {
        var isCrypto = type?.ToLower() == "crypto";
        var top = await _marketRepo.GetTopMarketAsync(isCrypto);
        ViewBag.Type = isCrypto ? "crypto" : "stock";
        return View("MarketHome", top);
    }

    [HttpGet]
    public async Task<IActionResult> SearchStocks(string searchTerm = "", string type = "")
    {
        var isCrypto = type?.ToLower() == "crypto";
        var results = await _marketRepo.SearchStocksAsync(searchTerm, isCrypto);
        return PartialView("_MarketCardsPartial", results); 
    }

    [HttpGet]
    [Route("api/market/highlow")]
    public async Task<IActionResult> GetHighLowChart(string ticker)
    {
        var data = await _marketRepo.GetHighLowHistoryAsync(ticker);
        return Json(data.Select(d => new
        {
            date = d.Date.ToString("yyyy-MM-dd"),
            high = d.High,
            low = d.Low
        }));
    }


}
