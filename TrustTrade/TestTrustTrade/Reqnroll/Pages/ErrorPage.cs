using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Threading;

namespace TestTrustTrade.Reqnroll.Pages
{
    public class ErrorPage : BasePage
    {
        // Locators
        private readonly By _errorCodeLocator = By.CssSelector(".error-code h1");
        private readonly By _errorTitleLocator = By.CssSelector(".error-code h2");
        private readonly By _homeButtonLocator = By.CssSelector(".bi-house-door");
        private readonly By _supportButtonLocator = By.CssSelector(".bi-headset");
        
        /// <summary>
        /// Gets the URL path for the error page
        /// </summary>
        protected override string PageUrl => "/Home/Error";
        
        /// <summary>
        /// Checks if the error page is loaded
        /// </summary>
        public override bool IsPageLoaded()
        {
            try
            {
                return Driver.Url.Contains("/Error") || 
                       Driver.FindElements(_errorCodeLocator).Count > 0 ||
                       Driver.PageSource.Contains("Investment Not Found");
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Gets the error code displayed on the page
        /// </summary>
        public string GetErrorCode()
        {
            try
            {
                return Driver.FindElement(_errorCodeLocator).Text;
            }
            catch
            {
                return string.Empty;
            }
        }
        
        /// <summary>
        /// Gets the error title displayed on the page
        /// </summary>
        public string GetErrorTitle()
        {
            try
            {
                return Driver.FindElement(_errorTitleLocator).Text;
            }
            catch
            {
                return string.Empty;
            }
        }
        
        /// <summary>
        /// Clicks the home button to return to the home page
        /// </summary>
        public bool ClickHomeButton()
        {
            try
            {
                Driver.FindElement(_homeButtonLocator).Click();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to click home button: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Clicks the contact support button
        /// </summary>
        public bool ClickContactSupportButton()
        {
            try
            {
                Driver.FindElement(_supportButtonLocator).Click();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to click contact support button: {ex.Message}");
                return false;
            }
        }
    }
}