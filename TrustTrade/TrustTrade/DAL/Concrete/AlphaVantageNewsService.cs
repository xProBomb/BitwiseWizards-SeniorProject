using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using TrustTrade.Models.AlphaVantage;
using TrustTrade.DAL.Abstract;
using TrustTrade.Helpers.JsonConverters;

namespace TrustTrade.DAL.Concrete
{
    /// <summary>
    /// Implementation of IFinancialNewsService using Alpha Vantage as the news source
    /// </summary>
    public class AlphaVantageNewsService : IFinancialNewsService
    {
        private readonly IFinancialNewsRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AlphaVantageNewsService> _logger;
        private readonly string _apiKey;

        /// <summary>
        /// Constructor for AlphaVantageNewsService
        /// </summary>
        public AlphaVantageNewsService(
            IFinancialNewsRepository repository,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<AlphaVantageNewsService> logger)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
            _apiKey = _configuration["AlphaVantage:ApiKey"];
        }

        /// <summary>
        /// Retrieves the latest financial news from the database
        /// </summary>
        public async Task<IEnumerable<FinancialNewsItem>> GetLatestNewsAsync(string category = null, int count = 10)
        {
            return await _repository.GetLatestNewsAsync(category, count);
        }

        /// <summary>
        /// Fetches news from Alpha Vantage and stores it in the database
        /// </summary>
        public async Task<int> SyncNewsFromAlphaVantageAsync(IEnumerable<string> stockSymbols, IEnumerable<string> cryptoSymbols)
        {
            int addedCount = 0;
            
            try
            {
                // Process stock symbols
                if (stockSymbols?.Any() == true)
                {
                    string tickers = string.Join(",", stockSymbols);
                    var stockNews = await FetchNewsFromAlphaVantageAsync(tickers, "Stock");
                    if (stockNews != null)
                    {
                        addedCount += await SaveNewsToDatabase(stockNews, "Stock");
                    }
                }

                // Process crypto symbols
                if (cryptoSymbols?.Any() == true)
                {
                    string tickers = string.Join(",", cryptoSymbols);
                    var cryptoNews = await FetchNewsFromAlphaVantageAsync(tickers, "Crypto");
                    if (cryptoNews != null)
                    {
                        addedCount += await SaveNewsToDatabase(cryptoNews, "Crypto");
                    }
                }

                return addedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing news from Alpha Vantage");
                throw;
            }
        }

        /// <summary>
        /// Makes the actual API call to Alpha Vantage
        /// </summary>
        private async Task<NewsResponse?> FetchNewsFromAlphaVantageAsync(string tickers, string category)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogError("Cannot fetch news: Alpha Vantage API key is missing.");
            return null;
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            tickers = tickers?.Trim();
            if (string.IsNullOrEmpty(tickers))
            {
                _logger.LogWarning("No tickers provided for {Category} news fetch.", category);
                return null;
            }

            // --- Define "Recent" Timespan ---
            var lookbackDuration = TimeSpan.FromDays(3); // Fetch news from the last 3 days
            var timeFrom = DateTime.UtcNow.Subtract(lookbackDuration);
            // Format required by Alpha Vantage API: YYYYMMDDTHHMM
            string timeFromParameter = timeFrom.ToString("yyyyMMddTHHmm", CultureInfo.InvariantCulture);
            // --- End Define "Recent" Timespan ---

            // --- Start of Specific URL Construction Logic ---
            string url;
            string apiTickersParameter;

            if (category == "Crypto")
            {
                // Use TOPICS for general crypto news, remove tickers parameter
                string cryptoTopics = "BLOCKCHAIN,TECHNOLOGY"; // Or adjust topics as needed
                url = $"https://www.alphavantage.co/query?function=NEWS_SENTIMENT&topics={cryptoTopics}&time_from={timeFromParameter}&limit=50&sort=LATEST&apikey={_apiKey}";
                // NOTE: We are no longer using the 'tickers' or 'apiTickersParameter' variable for the Crypto URL
            }
            else if (category == "Stock")
            {
                apiTickersParameter = tickers;
                 // Add time_from parameter
                url = $"https://www.alphavantage.co/query?function=NEWS_SENTIMENT&tickers={apiTickersParameter}&time_from={timeFromParameter}&limit=50&sort=LATEST&apikey={_apiKey}";
            }
            else
            {
                _logger.LogError("Unsupported category provided for news fetch: {Category}", category);
                return null;
            }
            // --- End of Specific URL Construction Logic ---

            string logUrl = url.Replace(_apiKey, "[API_KEY_MASKED]");
            _logger.LogInformation("Fetching {Category} news with URL: {LogUrl}", category, logUrl); // Log includes time_from now

            client.Timeout = TimeSpan.FromSeconds(30);
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Raw response for {Category} (first 500 chars): {ContentStart}", category, content.Substring(0, Math.Min(500, content.Length)));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error fetching news from Alpha Vantage: HTTP {StatusCode} - Response: {Content}", response.StatusCode, content);
                return null;
            }

            // Refined Check for Alpha Vantage API error/limit messages
            bool hasErrorMessage = content.Contains("\"Error Message\":");
            bool hasNoteMessage = content.Contains("\"Note\":");
            bool isApiLimitMessage = content.Contains("API call frequency") || content.Contains("call limit");

            if (hasErrorMessage || hasNoteMessage || isApiLimitMessage)
            {
                string issueType = hasErrorMessage ? "Error Message" : (hasNoteMessage ? "Note Message" : "API Limit Message");
                _logger.LogWarning("Alpha Vantage API returned an {IssueType}. Response: {Content}", issueType, content);
                if (isApiLimitMessage) { _logger.LogError("Alpha Vantage API limit likely exceeded for {Category}.", category); }
                return null;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                Converters = { new FloatFromStringConverter() }
            };

            NewsResponse? newsResponse = null;
            try
            {
                newsResponse = JsonSerializer.Deserialize<NewsResponse>(content, options);
                if (newsResponse?.Feed == null || newsResponse.Feed.Count == 0)
                { _logger.LogWarning("No news items found in the deserialized API response for {Category}.", category); }
                else
                { _logger.LogInformation("Successfully deserialized {FeedCount} news items for {Category}.", newsResponse.Feed.Count, category); }
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON Deserialization failed for {Category}. Path: {Path}. Raw content starts with: {ContentStart}",
                    category, jsonEx.Path ?? "N/A", content.Substring(0, Math.Min(1000, content.Length)));
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic error during deserialization process for {Category}. Raw content starts with: {ContentStart}", category, content.Substring(0, Math.Min(500, content.Length)));
                return null;
            }

            if (newsResponse?.Feed == null || !newsResponse.Feed.Any())
            { _logger.LogWarning("API call for {Category} successful but resulted in an empty feed after processing.", category); }

            return newsResponse;
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP Request Error fetching news for {Category} tickers: {Tickers}", category, tickers);
            return null;
        }
        catch (TaskCanceledException cancelEx)
        {
            _logger.LogError(cancelEx, "Timeout fetching news for {Category} tickers: {Tickers}", category, tickers);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching news for {Category} tickers: {Tickers}", category, tickers);
            return null;
        }
    }

        /// <summary>
        /// Saves fetched news items to the database
        /// </summary>
        private async Task<int> SaveNewsToDatabase(NewsResponse newsResponse, string category)
{
    if (newsResponse?.Feed == null || !newsResponse.Feed.Any())
    {
        _logger.LogWarning("SaveNewsToDatabase called for {Category} but Feed was null or empty.", category);
        return 0;
    }

    int addedCount = 0;
    _logger.LogInformation("Starting to process {FeedCount} {Category} news items for saving.", newsResponse.Feed.Count, category);

    foreach (var item in newsResponse.Feed)
    {
        // *** LOG BEFORE MAPPING ***
        _logger.LogDebug("Processing {Category} item: Title='{Title}', Url='{Url}'", category, item.Title, item.Url);

        var newsItem = new FinancialNewsItem
        {
            Title = item.Title,
            Summary = item.Summary,
            Url = item.Url, // Check this carefully for duplicates/variations
            TimePublished = item.GetTimePublished(),
            Authors = string.Join(", ", item.Authors ?? new List<string>()),
            Source = item.Source,
            Category = category, // Explicitly setting category here
            OverallSentimentScore = item.OverallSentimentScore,
            OverallSentimentLabel = item.OverallSentimentLabel,
            FetchedDate = DateTime.UtcNow,
            IsActive = true,
            Topics = item.Topics?.Select(t => new FinancialNewsTopic
            {
                TopicName = t.Topic,
                RelevanceScore = t.RelevanceScore
            }).ToList() ?? new List<FinancialNewsTopic>(),
            TickerSentiments = item.TickerSentiments?.Select(ts => new FinancialNewsTickerSentiment
            {
                TickerSymbol = ts.Ticker,
                TickerSentimentScore = ts.SentimentScore,
                TickerSentimentLabel = ts.SentimentLabel,
                RelevanceScore = ts.RelevanceScore
            }).ToList() ?? new List<FinancialNewsTickerSentiment>()
        };

         // *** LOG MAPPED ITEM (Optional but helpful) ***
         // try { _logger.LogTrace("Mapped {Category} FinancialNewsItem: {NewsItemJson}", category, JsonSerializer.Serialize(newsItem)); } catch {}

        bool wasAdded = false;
        try
        {
            // *** LOG BEFORE AddNewsItemAsync ***
            _logger.LogDebug("Attempting AddNewsItemAsync for {Category} URL: {Url}", category, newsItem.Url);
            wasAdded = await _repository.AddNewsItemAsync(newsItem);
            // *** LOG RESULT of AddNewsItemAsync ***
            _logger.LogDebug("Result of AddNewsItemAsync for {Category} URL {Url}: {WasAdded}", category, newsItem.Url, wasAdded);

            if (wasAdded)
            {
                addedCount++;
            }
        }
        catch (Exception ex)
        {
            // Catch errors during the Add attempt itself
             _logger.LogError(ex, "Error during AddNewsItemAsync for {Category} URL: {Url}", category, newsItem.Url);
             // Continue to next item potentially? Or rethrow depending on desired behavior.
        }
    } // End foreach loop

    // *** LOG BEFORE SaveChangesAsync ***
    _logger.LogInformation("Finished processing {Category} items. Added Count before SaveChanges: {AddedCount}", category, addedCount);

    if (addedCount > 0)
    {
        try // *** Add try-catch around SaveChangesAsync ***
        {
            await _repository.SaveChangesAsync();
            _logger.LogInformation("Successfully saved {AddedCount} new {Category} news items to database.", addedCount, category);
        }
        catch (DbUpdateException dbEx) // Catch EF Core update exceptions
        {
             _logger.LogError(dbEx, "DbUpdateException saving {Category} news. AddedCount was {AddedCount}. Check inner exception. First entry details: {EntryDetails}",
                category, addedCount, dbEx.Entries.FirstOrDefault()?.DebugView.LongView ?? "N/A");
             // You might want to log all entries dbEx.Entries
             addedCount = 0; // Reset count as save failed
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Generic error saving {Category} news. AddedCount was {AddedCount}", category, addedCount);
             addedCount = 0; // Reset count as save failed
        }
    }
    else
    {
         _logger.LogInformation("No new {Category} items were marked for addition, skipping SaveChanges.", category);
    }

    return addedCount;
}
    }
}