using OpenQA.Selenium;

namespace TestTrustTrade.Reqnroll.Pages
{
    /// <summary>
    /// Page object for the Notifications page
    /// </summary>
    public class LandingPage : BasePage
    {
        // Locators
        private readonly By _enterSite = By.Id("NavToIndex");
        private readonly By _widgetTitle = By.Id("financialNewsWidget");
        
        /// <summary>
        /// Gets the URL path for the Landing page
        /// </summary>
        protected override string PageUrl => "/Home/Landing";
        
        /// <summary>
        /// Checks if the Landing page is loaded
        /// </summary>
        /// <returns>True if the page is loaded</returns>
        public override bool IsPageLoaded()
        {
            try
            {
                Driver.Navigate().GoToUrl(BaseUrl + "/Home/Landing");
                // Wait for the page to load
                Thread.Sleep(1000);
                return WaitForElementVisible(_enterSite) != null || 
                       Driver.PageSource.Contains("You are not on the correct page");
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Checks if the widget title is visible
        /// </summary>
        /// <returns>True if the widget title is visible</returns>
        public bool IsWidgetTitleVisible() => WaitForElementVisible(_widgetTitle) != null;
        
        /// <summary>
        /// Click the enter site button 
        /// </summary>
        public void EnterSite()
        {
            SafeClick(_enterSite);
            // Wait briefly for animation
            Thread.Sleep(500);

            WaitForElementVisible(_widgetTitle);
        }
    }
}