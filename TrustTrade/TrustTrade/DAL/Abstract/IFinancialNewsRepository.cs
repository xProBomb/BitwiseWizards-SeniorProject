using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract
{
    /// <summary>
    /// Repository interface for financial news data access
    /// </summary>
    public interface IFinancialNewsRepository
    {
        /// <summary>
        /// Gets the latest financial news from the database
        /// </summary>
        /// <param name="category">Optional category filter (Stock or Crypto)</param>
        /// <param name="count">Maximum number of news items to return</param>
        /// <returns>Collection of financial news items</returns>
        Task<IEnumerable<FinancialNewsItem>> GetLatestNewsAsync(string category = null, int count = 10);

        /// <summary>
        /// Adds a new financial news item to the database if it doesn't already exist
        /// </summary>
        /// <param name="newsItem">The news item to add</param>
        /// <returns>True if the item was added, false if it already exists</returns>
        Task<bool> AddNewsItemAsync(FinancialNewsItem newsItem);

        /// <summary>
        /// Checks if a news item with the given URL already exists in the database
        /// </summary>
        /// <param name="url">The URL to check</param>
        /// <returns>True if the item exists, false otherwise</returns>
        Task<bool> NewsItemExistsAsync(string url);

        /// <summary>
        /// Saves all pending changes to the database
        /// </summary>
        Task SaveChangesAsync();
    }
}