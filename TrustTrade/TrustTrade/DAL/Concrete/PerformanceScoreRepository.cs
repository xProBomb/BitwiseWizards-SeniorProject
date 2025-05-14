using Microsoft.EntityFrameworkCore;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;

namespace TrustTrade.DAL.Concrete
{
    /// <summary>
    /// Implementation of performance score calculations and requirements
    /// </summary>
    public class PerformanceScoreRepository : IPerformanceScoreRepository
    {
        private readonly TrustTradeDbContext _context;
        private readonly IHoldingsRepository _holdingsRepository;
        private readonly ILogger<PerformanceScoreRepository> _logger;
        
        // Weight constants (inverse - lower means more important)
        private const decimal HOLDINGS_IN_GREEN_WEIGHT = 1;
        private const decimal UPVOTES_WEIGHT = 3;
        private const decimal PORTFOLIO_PERFORMANCE_WEIGHT = 5;
        
        public PerformanceScoreRepository(
            TrustTradeDbContext context,
            IHoldingsRepository holdingsRepository,
            ILogger<PerformanceScoreRepository> logger)
        {
            _context = context;
            _holdingsRepository = holdingsRepository;
            _logger = logger;
        }
        
        /// <inheritdoc />
        public async Task<(decimal Score, bool IsRated, Dictionary<string, decimal> Breakdown)> CalculatePerformanceScoreAsync(int userId)
        {
            try
            {
                // Get user with related data
                var user = await _context.Users
                    .Include(u => u.Posts)
                    .Include(u => u.PlaidConnections)
                        .ThenInclude(pc => pc.InvestmentPositions)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                    
                if (user == null)
                {
                    _logger.LogWarning("User not found for performance score calculation: {UserId}", userId);
                    return (0, false, new Dictionary<string, decimal>());
                }
                
                if (!MeetsMinimumRequirements(user))
                {
                    _logger.LogInformation("User {UserId} does not meet minimum requirements for performance score", userId);
                    return (0, false, new Dictionary<string, decimal>());
                }
                
                // Get latest post with portfolio value
                var latestPostWithValue = user.Posts
                    .Where(p => p.PortfolioValueAtPosting.HasValue)
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefault();
                
                // Get all holdings
                var holdings = await _holdingsRepository.GetHoldingsForUserAsync(userId);
                
                // Calculate component scores
                var holdingsInGreenScore = CalculateHoldingsInGreenScore(holdings);
                var upvotesScore = CalculateUpvotesScore(user.Posts);
                var portfolioPerformanceScore = CalculatePortfolioPerformanceScore(
                    latestPostWithValue, holdings);
                
                _logger.LogDebug("Score components - Holdings: {HoldingsScore}, Upvotes: {UpvotesScore}, Performance: {PerformanceScore}", 
                    holdingsInGreenScore, upvotesScore, portfolioPerformanceScore);
                
                // Apply inverse weighting (lower weight = more important)
                decimal weightedHoldingsScore = holdingsInGreenScore / HOLDINGS_IN_GREEN_WEIGHT;
                decimal weightedUpvotesScore = upvotesScore / UPVOTES_WEIGHT;
                decimal weightedPortfolioScore = portfolioPerformanceScore / PORTFOLIO_PERFORMANCE_WEIGHT;
                
                // Calculate weighted average
                decimal totalWeight = (1/HOLDINGS_IN_GREEN_WEIGHT) + (1/UPVOTES_WEIGHT) + (1/PORTFOLIO_PERFORMANCE_WEIGHT);
                decimal finalScore = (weightedHoldingsScore + weightedUpvotesScore + weightedPortfolioScore) / totalWeight;
                
                // Ensure score is in 0-100 range
                finalScore = Math.Clamp(finalScore, 0, 100);
                
                _logger.LogInformation("Calculated performance score {Score} for user {UserId}", finalScore, userId);
                
                // Create score breakdown
                var breakdown = new Dictionary<string, decimal>
                {
                    { "Holdings in Green", holdingsInGreenScore },
                    { "Community Feedback", upvotesScore },
                    { "Portfolio Performance", portfolioPerformanceScore }
                };
                
                return (finalScore, true, breakdown);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating performance score for user {UserId}", userId);
                return (0, false, new Dictionary<string, decimal>());
            }
        }
        
        /// <inheritdoc />
        public bool MeetsMinimumRequirements(User user)
        {
            // User must have Plaid connected, at least one post, and at least one holding
            bool meetsRequirements = user.PlaidEnabled == true && 
                                    user.Posts.Count > 0 && 
                                    user.PlaidConnections.Any(pc => pc.InvestmentPositions.Count > 0);
                                    
            return meetsRequirements;
        }
        
        /// <summary>
        /// Calculates score component based on percentage of holdings in green and their return percentages
        /// </summary>
        private decimal CalculateHoldingsInGreenScore(List<InvestmentPosition> holdings)
        {
            if (holdings == null || !holdings.Any())
                return 0;
            
            // Calculate how many holdings are "in the green" (current value > cost basis)
            int greenCount = 0;
            int totalCount = holdings.Count;
            decimal totalGainPercentage = 0;
            
            foreach (var holding in holdings)
            {
                decimal costValue = holding.Quantity * holding.CostBasis;
                decimal currentValue = holding.Quantity * holding.CurrentPrice;
                
                if (currentValue > costValue)
                {
                    greenCount++;
                    
                    // Calculate gain percentage for this holding
                    if (costValue > 0)
                    {
                        decimal gainPercentage = (currentValue - costValue) / costValue * 100;
                        totalGainPercentage += gainPercentage;
                    }
                }
            }
            
            // Calculate percentage of holdings in green
            decimal percentInGreen = (decimal)greenCount / totalCount * 100;
            
            // Calculate average gain percentage for green holdings
            decimal avgGainPercentage = greenCount > 0 ? totalGainPercentage / greenCount : 0;
            
            // Final score is a combination of percentage in green and average gain
            // Normalize to 0-100 scale
            decimal rawScore = (percentInGreen * 0.7m) + (Math.Min(avgGainPercentage, 30) * 1m);
            return Math.Min(rawScore, 100);
        }
        
        /// <summary>
        /// Calculates score component based on community engagement (likes/upvotes)
        /// </summary>
        private decimal CalculateUpvotesScore(ICollection<Post> posts)
        {
            // Since upvotes aren't implemented yet, this is a placeholder
            // In the future, this would calculate score based on likes per post
            
            // For now, just check if they have posts
            if (posts == null || !posts.Any())
                return 0;
                
            // Placeholder: Give a minimum score of 50 just for having posts
            return 50;
        }
        
        /// <summary>
        /// Calculates score component based on portfolio value change since last post
        /// </summary>
        private decimal CalculatePortfolioPerformanceScore(Post latestPostWithValue, List<InvestmentPosition> holdings)
        {
            // If no post with portfolio value or no holdings, return zero
            if (latestPostWithValue?.PortfolioValueAtPosting == null || !holdings.Any())
                return 0;
                
            // Calculate current total portfolio value
            decimal currentValue = holdings.Sum(h => h.Quantity * h.CurrentPrice);
            
            // Calculate percentage change since last post
            decimal previousValue = latestPostWithValue.PortfolioValueAtPosting.Value;
            
            if (previousValue <= 0)
                return 0;
                
            decimal percentageChange = (currentValue - previousValue) / previousValue * 100;
            
            // Map percentage change to a 0-100 score
            // Positive changes score higher
            if (percentageChange >= 0)
            {
                // Cap at 20% for max score
                return Math.Min(50 + (percentageChange * 2.5m), 100);
            }
            else
            {
                // Negative changes score lower, but not as severely
                // -20% would be score of 0
                return Math.Max(50 + (percentageChange * 2.5m), 0);
            }
        }

        public async Task<decimal> CalculateStockPerformanceScoreAsync(string tickerSymbol, int days = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.Date.AddDays(-days);

                var history = await _context.StockHistories
                    .Where(s => s.TickerSymbol == tickerSymbol && s.Date >= cutoffDate)
                    .OrderBy(s => s.Date)
                    .ToListAsync();

                if (history.Count < 2)
                {
                    _logger.LogInformation("Not enough data for {Ticker} to calculate performance", tickerSymbol);
                    return 0;
                }

                var returns = new List<decimal>();
                for (int i = 1; i < history.Count; i++)
                {
                    decimal previous = history[i - 1].HighPrice;
                    decimal current = history[i].HighPrice;

                    if (previous > 0)
                    {
                        decimal dailyReturn = (current - previous) / previous;
                        returns.Add(dailyReturn);
                    }
                }

                if (returns.Count == 0)
                    return 0;

                decimal avgDailyReturn = returns.Average();
                decimal stdDev = (decimal)Math.Sqrt((double)returns.Average(r => (r - avgDailyReturn) * (r - avgDailyReturn)));

                // Annualize average return and volatility (assuming 252 trading days)
                decimal annualizedReturn = avgDailyReturn * 252;
                decimal annualizedVolatility = stdDev * (decimal)Math.Sqrt(252);

                // Performance score: Sharpe-like ratio (without risk-free rate)
                decimal rawScore = annualizedReturn / (annualizedVolatility + 0.0001m); // prevent div by 0

                

// Compress and normalize
                decimal safeRaw = Math.Min(Math.Max(rawScore, -1), 5); // limit extreme values

// Logarithmic compression to reduce skew at high end
                    decimal compressed = (decimal)Math.Log10(1 + (double)Math.Max(0, safeRaw));

                    // Scale up with more breathing room
                    decimal normalizedScore = Math.Clamp(compressed * 100 / 1.2m, 0, 100); 

                _logger.LogDebug("Stock {Ticker}: AvgReturn={Avg}, Volatility={Vol}, RawScore={Raw}, FinalScore={Final}",
                    tickerSymbol, avgDailyReturn, annualizedVolatility, rawScore, normalizedScore);

                return normalizedScore;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate detailed stock performance score for {Ticker}", tickerSymbol);
                return 0;
            }
        }

    }
}