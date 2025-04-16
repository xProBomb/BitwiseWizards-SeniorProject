using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using TrustTrade.DAL.Abstract;

namespace TrustTrade.Services.Background
{
    /// <summary>
    /// Background service that periodically syncs financial news data from Alpha Vantage
    /// </summary>
    public class FinancialNewsBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FinancialNewsBackgroundService> _logger;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _refreshInterval;
        
        /// <summary>
        /// Constructor for FinancialNewsBackgroundService
        /// </summary>
        public FinancialNewsBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<FinancialNewsBackgroundService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
            
            // Get refresh interval from configuration, default to once per day
            string intervalStr = _configuration["FinancialNewsSettings:RefreshIntervalHours"];
            if (!double.TryParse(intervalStr, out double intervalHours) || intervalHours < 1)
            {
                intervalHours = 24; // Default to daily
            }
            else if (intervalHours < 4)
            {
                // If the interval is set to less than 4 hours, force it to 4 hours
                // to avoid hitting Alpha Vantage API limits (25 calls per day with free API)
                _logger.LogWarning($"RefreshIntervalHours was set to {intervalHours} which is too frequent for Alpha Vantage free API. Setting to 4 hours minimum.");
                intervalHours = 4;
            }
            _refreshInterval = TimeSpan.FromHours(intervalHours);
            _logger.LogInformation($"Financial news will refresh every {intervalHours} hours");
        }

        /// <summary>
        /// Main execution method for the background service
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Financial News Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SyncNewsAsync();
                    await Task.Delay(_refreshInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Normal shutdown, don't log as error
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while syncing financial news");
                    // Wait a bit before retrying
                    await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
                }
            }

            _logger.LogInformation("Financial News Background Service stopped");
        }

        /// <summary>
        /// Performs the actual news sync operation
        /// </summary>
        private async Task SyncNewsAsync()
        {
            _logger.LogInformation("Starting financial news sync");
            
            // Create a scope for dependency injection
            using (var scope = _serviceProvider.CreateScope())
            {
                var newsService = scope.ServiceProvider.GetRequiredService<IFinancialNewsService>();
                
                // Get stock symbols to monitor from configuration
                var stockSymbols = _configuration.GetSection("FinancialNewsSettings:StockSymbols")
                    .Get<List<string>>() ?? new List<string> { "MSFT", "AAPL", "GOOGL", "AMZN", "META" };
                
                // Get crypto symbols to monitor from configuration
                var cryptoSymbols = _configuration.GetSection("FinancialNewsSettings:CryptoSymbols")
                    .Get<List<string>>() ?? new List<string> { "BTC", "ETH", "SOL", "XRP", "ADA" };
                
                _logger.LogInformation($"Syncing news for stocks: {string.Join(", ", stockSymbols)}");
                _logger.LogInformation($"Syncing news for crypto: {string.Join(", ", cryptoSymbols)}");
                
                try
                {
                    int addedCount = await newsService.SyncNewsFromAlphaVantageAsync(stockSymbols, cryptoSymbols);
                    _logger.LogInformation($"Financial news sync completed. Added {addedCount} new items.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing financial news");
                    throw; // Rethrow to let the outer handler manage retries
                }
            }
        }
    }
}