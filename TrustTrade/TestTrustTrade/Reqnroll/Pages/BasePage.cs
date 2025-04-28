using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using TestTrustTrade.Reqnroll.Services;

namespace TestTrustTrade.Reqnroll.Pages
{
    /// <summary>
    /// Base page class for all page objects
    /// </summary>
    public abstract class BasePage
    {
        protected readonly IWebDriver Driver;
        protected readonly WebDriverWait Wait;
        protected readonly string BaseUrl;
        
        /// <summary>
        /// Base constructor for page objects
        /// </summary>
        protected BasePage()
        {
            Driver = WebDriverSingleton.Instance;
            Wait = WebDriverSingleton.GetWait();
            BaseUrl = WebDriverSingleton.BaseUrl;
        }
        
        /// <summary>
        /// Gets the URL path for this page
        /// </summary>
        protected abstract string PageUrl { get; }
        
        /// <summary>
        /// Navigates to this page
        /// </summary>
        public virtual void NavigateTo()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}{PageUrl}");
        }
        
        /// <summary>
        /// Waits for the page to be loaded
        /// </summary>
        public abstract bool IsPageLoaded();
        
        /// <summary>
        /// Takes a screenshot of the current page
        /// </summary>
        /// <param name="fileName">File name for the screenshot</param>
        /// <returns>Path to the saved screenshot</returns>
        public string TakeScreenshot(string fileName)
        {
            var screenshotDriver = (ITakesScreenshot)Driver;
            var screenshot = screenshotDriver.GetScreenshot();
            var filePath = $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            screenshot.SaveAsFile(filePath);
            return filePath;
        }
        
        /// <summary>
        /// Waits for element to be visible
        /// </summary>
        /// <param name="by">Element locator</param>
        /// <param name="timeoutInSeconds">Timeout in seconds</param>
        /// <returns>The WebElement</returns>
        protected IWebElement WaitForElementVisible(By by, int timeoutInSeconds = 10)
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutInSeconds));
            return wait.Until(d => {
                var element = d.FindElement(by);
                return element.Displayed ? element : null;
            });
        }
        
        /// <summary>
        /// Waits for element to be clickable
        /// </summary>
        /// <param name="by">Element locator</param>
        /// <param name="timeoutInSeconds">Timeout in seconds</param>
        /// <returns>The WebElement</returns>
        protected IWebElement WaitForElementClickable(By by, int timeoutInSeconds = 10)
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutInSeconds));
            return wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(by));
        }
        
        /// <summary>
        /// Safely clicks on an element with wait and retry
        /// </summary>
        /// <param name="by">Element locator</param>
        /// <param name="retries">Number of retries</param>
        protected void SafeClick(By by, int retries = 3)
        {
            Exception lastException = null;
            
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    var element = WaitForElementClickable(by);
                    element.Click();
                    return;
                }
                catch (StaleElementReferenceException ex)
                {
                    lastException = ex;
                    // Wait a bit before retry
                    System.Threading.Thread.Sleep(500);
                }
                catch (ElementClickInterceptedException ex)
                {
                    lastException = ex;
                    // Wait a bit longer before retry
                    System.Threading.Thread.Sleep(1000);
                }
            }
            
            // If we get here, all retries failed
            throw new Exception($"Failed to click element {by} after {retries} retries", lastException);
        }
    }
}