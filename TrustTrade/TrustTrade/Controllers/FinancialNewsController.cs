using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;

namespace TrustTrade.Controllers
{
    /// <summary>
    /// Controller for financial news-related requests
    /// </summary>
    public class FinancialNewsController : Controller
    {
        private readonly IFinancialNewsService _newsService;
        private readonly ILogger<FinancialNewsController> _logger;
        private const int PageSize = 20;

        /// <summary>
        /// Constructor for FinancialNewsController
        /// </summary>
        public FinancialNewsController(
            IFinancialNewsService newsService,
            ILogger<FinancialNewsController> logger)
        {
            _newsService = newsService;
            _logger = logger;
        }

        /// <summary>
        /// Displays the financial news page with optional filtering and search
        /// </summary>
        /// <param name="category">Optional category filter (Stock/Crypto)</param>
        /// <param name="searchTerm">Optional search term</param>
        /// <param name="count">Number of items to display</param>
        public async Task<IActionResult> Index(string category = null, string searchTerm = null)
        {
            try
            {
                IEnumerable<FinancialNewsItem> newsItems;
                // Normalize empty/whitespace search term to null for clearer logic
                string effectiveSearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm;

                // Determine if we should use search or regular retrieval
                if (effectiveSearchTerm != null)
                {
                    newsItems = await _newsService.SearchNewsAsync(effectiveSearchTerm, category, PageSize);
                    ViewBag.SearchTerm = effectiveSearchTerm; // Store the actual search term used
                }
                else
                {
                    newsItems = await _newsService.GetLatestNewsAsync(category, PageSize);
                    ViewBag.SearchTerm = null; // Explicitly set to null when not searching
                }

                ViewBag.CurrentCategory = category; // Keep current category regardless of search
                ViewBag.PageSize = PageSize;
                ViewBag.InitialItemCount = newsItems.Count();
                return View(newsItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving financial news. Category: {Category}, SearchTerm: {SearchTerm}",
                    category, searchTerm);
                // Consider returning a specific error view or model
                return View("Error", new ErrorViewModel { ErrorMessage = "Failed to load financial news." });
            }
        }
        
        // *** ACTION FOR INFINITE SCROLL ***
        [HttpGet]
        public async Task<IActionResult> LoadMoreNews(int page = 2, string category = null, string searchTerm = null)
        {
             if (page < 2) page = 2; // Ensure we don't re-request page 1

            try
            {
                _logger.LogInformation("Loading more news - Page: {Page}, Category: {Category}, Search: {SearchTerm}", page, category ?? "All", searchTerm ?? "None");

                IEnumerable<FinancialNewsItem> newsItems;
                string effectiveSearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm;

                // Calculate the number of items to skip
                int skip = (page - 1) * PageSize;

                // --- IMPORTANT: We need methods in the service/repo that support skipping ---
                // For demonstration, let's assume the existing methods can be adapted or new ones exist.
                // If not, you'll need to modify IFinancialNewsService and its implementation
                // to accept 'skip' and 'take' parameters.

                // Placeholder: Simulating skip/take logic. Replace with actual service calls.
                 if (effectiveSearchTerm != null)
                 {
                     // You'd ideally have a service method like:
                     // newsItems = await _newsService.SearchNewsPagedAsync(effectiveSearchTerm, category, skip, PageSize);
                     // For now, we fetch more and manually skip/take (less efficient)
                     var allMatching = await _newsService.SearchNewsAsync(effectiveSearchTerm, category, count: 500); // Fetch a large number initially
                     newsItems = allMatching.Skip(skip).Take(PageSize);
                 }
                 else
                 {
                     // You'd ideally have a service method like:
                     // newsItems = await _newsService.GetLatestNewsPagedAsync(category, skip, PageSize);
                     // For now, we fetch more and manually skip/take (less efficient)
                     var allLatest = await _newsService.GetLatestNewsAsync(category, count: 500); // Fetch a large number initially
                     newsItems = allLatest.Skip(skip).Take(PageSize);
                 }


                // Pass the fetched items (only for the current page) to the partial view
                return PartialView("_NewsItemsPartial", newsItems);
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error loading more news. Page: {Page}, Category: {Category}, Search: {SearchTerm}",
                     page, category, searchTerm);
                 // Return an empty result or an error partial if needed
                 // Returning empty prevents JS from trying infinitely on error
                 return PartialView("_NewsItemsPartial", Enumerable.Empty<FinancialNewsItem>());
            }
        }

        /// <summary>
        /// Returns a partial view of financial news for inclusion on other pages
        /// </summary>
        public async Task<IActionResult> NewsWidget(string? category = null, int count = 5) // Allow nullable category
        {
            try
            {
                // Get latest news stored in YOUR database
                var newsItems = await _newsService.GetLatestNewsAsync(category, count);
                ViewBag.CurrentCategory = category; // Pass category for tab highlighting
                return PartialView("_NewsWidget", newsItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving financial news for widget");
                // Return a simple message, the JS in Index.cshtml handles displaying it
                return Content("<div class='alert alert-warning p-3 m-0 border-0'>Unable to load financial news at this time.</div>");
            }
        }
        
    }
}