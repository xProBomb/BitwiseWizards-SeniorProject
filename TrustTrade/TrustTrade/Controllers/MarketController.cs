using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using TrustTrade.ViewModels;




public class MarketController : Controller
{
    private readonly IMarketRepository _marketRepo;

    public MarketController(IMarketRepository marketRepo)
    {
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

}
