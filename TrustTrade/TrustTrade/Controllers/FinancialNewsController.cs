using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TrustTrade.DAL.Abstract;

namespace TrustTrade.Controllers
{
    /// <summary>
    /// Controller for financial news-related requests
    /// </summary>
    public class FinancialNewsController : Controller
    {
        private readonly IFinancialNewsService _newsService;
        private readonly ILogger<FinancialNewsController> _logger;

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
        /// Displays the financial news page
        /// </summary>
        public async Task<IActionResult> Index(string category = null)
        {
            try
            {
                var newsItems = await _newsService.GetLatestNewsAsync(category, 20);
                ViewBag.CurrentCategory = category;
                return View(newsItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving financial news");
                return View("Error");
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