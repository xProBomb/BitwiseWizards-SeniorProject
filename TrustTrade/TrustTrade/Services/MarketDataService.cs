using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TrustTrade.ViewModels;

public class MarketDataService : IMarketDataService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public MarketDataService(IHttpClientFactory httpClientFactory, IOptions<MarketDataOptions> options)
    {
        _httpClient = httpClientFactory.CreateClient();
        _apiKey = options.Value.ApiKey;
    }

    public async Task<StockViewModel?> GetStockQuoteAsync(string ticker)
{
    var url = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={ticker}&apikey={_apiKey}";
    var response = await _httpClient.GetAsync(url);

    if (!response.IsSuccessStatusCode)
        return null;

    var json = await response.Content.ReadAsStringAsync();
    var parsed = JsonDocument.Parse(json);

    if (!parsed.RootElement.TryGetProperty("Global Quote", out var quote) || quote.GetRawText() == "{}")
    {
       
        Console.WriteLine($"⚠️ No data returned for ticker: {ticker}");
        return null;
    }

    try
    {
        return new StockViewModel
        {
            Ticker = quote.GetProperty("01. symbol").GetString(),
            Name = "", // No name in Global Quote
            Price = decimal.TryParse(quote.GetProperty("05. price").GetString(), out var price) ? price : 0,
            Change = decimal.TryParse(quote.GetProperty("09. change").GetString(), out var change) ? change : 0
        };
    }
    catch
    {
        Console.WriteLine($"⚠️ Failed to parse data for ticker: {ticker}");
        return null;
    }
}

}
