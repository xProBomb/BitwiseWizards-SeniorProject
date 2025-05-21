using OpenQA.Selenium;
using System;

namespace TestTrustTrade.Reqnroll.Pages
{
    public class HomePage : BasePage
    {
        // Locators for key elements that indicate we're on the home page
        private readonly By _postsFeed = By.CssSelector(".post-preview, .list-group-item");
        private readonly By _welcomeHeader = By.CssSelector("h2");
        
        /// <summary>
        /// Gets the URL path for the home page
        /// </summary>
        protected override string PageUrl => "/"; // Root path for home
        
        /// <summary>
        /// Checks if the home page is loaded
        /// </summary>
        public override bool IsPageLoaded()
        {
            try
            {
                // Check if we're on a URL that could be home
                bool isHomeUrl = Driver.Url.EndsWith("/") || 
                                 Driver.Url.EndsWith("/Home") || 
                                 Driver.Url.EndsWith("/Home/Index");
                
                // Check for key home page elements
                bool hasHomeElements = Driver.FindElements(_postsFeed).Count > 0 || 
                                       Driver.FindElements(_welcomeHeader).Count > 0;
                
                return isHomeUrl || hasHomeElements;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if home page is loaded: {ex.Message}");
                return false;
            }
        }
    }
}