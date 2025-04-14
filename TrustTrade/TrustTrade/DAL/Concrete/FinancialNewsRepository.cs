using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;

namespace TrustTrade.DAL.Concrete
{
    /// <summary>
    /// Repository implementation for financial news data access
    /// </summary>
    public class FinancialNewsRepository : IFinancialNewsRepository
    {
        private readonly TrustTradeDbContext _dbContext;
        private readonly ILogger<FinancialNewsRepository> _logger;

        /// <summary>
        /// Constructor for FinancialNewsRepository
        /// </summary>
        public FinancialNewsRepository(
            TrustTradeDbContext dbContext,
            ILogger<FinancialNewsRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Gets the latest financial news from the database
        /// </summary>
        public async Task<IEnumerable<FinancialNewsItem>> GetLatestNewsAsync(string category = null, int count = 10)
        {
            IQueryable<FinancialNewsItem> query = _dbContext.FinancialNewsItems
                .Where(n => n.IsActive)
                .Include(n => n.Topics)
                .Include(n => n.TickerSentiments);

            // Apply category filter if provided
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(n => n.Category == category);
            }
            
            // Now apply ordering and take the required count
            var orderedQuery = query.OrderByDescending(n => n.TimePublished);

            return await orderedQuery.Take(count).ToListAsync();
        }

        /// <summary>
        /// Adds a new financial news item to the database if it doesn't already exist
        /// </summary>
        public async Task<bool> AddNewsItemAsync(FinancialNewsItem newsItem)
        {
            if (await NewsItemExistsAsync(newsItem.Url))
            {
                return false;
            }

            try
            {
                await _dbContext.FinancialNewsItems.AddAsync(newsItem);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding news item: {newsItem.Title}");
                return false;
            }
        }

        /// <summary>
        /// Checks if a news item with the given URL already exists in the database
        /// </summary>
        public async Task<bool> NewsItemExistsAsync(string url)
        {
            return await _dbContext.FinancialNewsItems.AnyAsync(n => n.Url == url);
        }

        /// <summary>
        /// Saves all pending changes to the database
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}