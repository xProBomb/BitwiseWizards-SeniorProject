using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract
{
    /// <summary>
    /// Interface for accessing financial news data
    /// </summary>
    public interface IFinancialNewsService
    {
        /// <summary>
        /// Fetches the latest financial news from the database
        /// </summary>
        /// <param name="category">Optional category filter (Stock or Crypto)</param>
        /// <param name="count">Maximum number of news items to return</param>
        /// <returns>Collection of financial news items</returns>
        Task<IEnumerable<FinancialNewsItem>> GetLatestNewsAsync(string category = null, int count = 15);

        /// <summary>
        /// Synchronizes financial news data from Alpha Vantage to the local database
        /// This method is intended to be called by a scheduled job, not directly by controllers
        /// </summary>
        /// <param name="stockSymbols">Stock ticker symbols to fetch news for</param>
        /// <param name="cryptoSymbols">Cryptocurrency symbols to fetch news for</param>
        /// <returns>Number of new news items added to the database</returns>
        Task<int> SyncNewsFromAlphaVantageAsync(IEnumerable<string> stockSymbols, IEnumerable<string> cryptoSymbols);
        
        /// <summary>
        /// Searches for news items containing the specified term
        /// </summary>
        /// <param name="searchTerm">Term to search for in title and summary</param>
        /// <param name="category">Optional category filter</param>
        /// <param name="count">Maximum number of items to return</param>
        /// <returns>Collection of matching news items</returns>
        Task<IEnumerable<FinancialNewsItem>> SearchNewsAsync(string searchTerm, string category = null, int count = 10);
    }
}