using OpenQA.Selenium;
using Reqnroll;
using TestTrustTrade.Reqnroll.Pages;
using TestTrustTrade.Reqnroll.Services;

namespace TestTrustTrade.Reqnroll.StepDefinitions
{
    [Binding]
    public class ErrorPageStepDefinitions : IDisposable
    {
        private readonly LoginService _loginService;
        private readonly IWebDriver _driver;
        private readonly ErrorPage _errorPage;
        private readonly HomePage _homePage;

        public ErrorPageStepDefinitions()
        {
            // Get driver reference from the singleton
            _driver = WebDriverSingleton.Instance;
            
            // Initialize services and page objects
            _loginService = new LoginService();
            _errorPage = new ErrorPage();
            _homePage = new HomePage();
        }

        [Given(@"I am using the TrustTrade application")]
        public void GivenIAmUsingTheTrustTradeApplication()
        {
            // Navigate to home page to ensure we're using the application
            _driver.Navigate().GoToUrl(WebDriverSingleton.BaseUrl);
        }

        [When(@"I navigate to an invalid URL ""(.*)""")]
        public void WhenINavigateToAnInvalidURL(string invalidPath)
        {
            // Navigate to the invalid URL
            _driver.Navigate().GoToUrl($"{WebDriverSingleton.BaseUrl}{invalidPath}");
            
            // Wait briefly for the redirect to happen
            System.Threading.Thread.Sleep(1000);
        }

        [Then(@"I should be redirected to the error page")]
        public void ThenIShouldBeRedirectedToTheErrorPage()
        {
            // Verify we're on an error page (URL contains "Error" or has specific element)
            Assert.That(_driver.Url.Contains("/Error") || _driver.PageSource.Contains("Investment Not Found"), 
                Is.True, "Not redirected to error page");
            
            // Verify the error page loaded
            Assert.That(_errorPage.IsPageLoaded(), Is.True, "Error page failed to load");
        }

        [Then(@"I should see the error code ""(.*)""")]
        public void ThenIShouldSeeTheErrorCode(string errorCode)
        {
            // Verify the error code is visible
            string pageSource = _driver.PageSource;
            Assert.That(pageSource.Contains(errorCode), Is.True, $"Error code {errorCode} not found on page");
        }

        [Then(@"I should see the text ""(.*)""")]
        public void ThenIShouldSeeTheText(string text)
        {
            // Verify the specified text is visible
            string pageSource = _driver.PageSource;
            Assert.That(pageSource.Contains(text), Is.True, $"Text '{text}' not found on page");
        }

        [Then(@"I should see a link to contact support")]
        public void ThenIShouldSeeALinkToContactSupport()
        {
            // Verify the contact support link exists
            bool supportLinkExists = _driver.FindElements(By.CssSelector(".bi-headset")).Count > 0;
            Assert.That(supportLinkExists, Is.True, "Contact support link not found");
        }

        [Then(@"I should see a home button")]
        public void ThenIShouldSeeAHomeButton()
        {
            // Verify the home button exists
            bool homeButtonExists = _driver.FindElements(By.CssSelector(".bi-house-door")).Count > 0;
            Assert.That(homeButtonExists, Is.True, "Home button not found");
        }

        [Given(@"I am on the error page")]
        public void GivenIAmOnTheErrorPage()
        {
            // Navigate to a known invalid URL to get to the error page
            _driver.Navigate().GoToUrl($"{WebDriverSingleton.BaseUrl}/NonExistentPath");
            
            // Wait briefly for the redirect to happen
            System.Threading.Thread.Sleep(1000);
            
            // Verify the error page loaded
            Assert.That(_errorPage.IsPageLoaded(), Is.True, "Error page failed to load");
        }

        [When(@"I click the ""(.*)"" button")]
        public void WhenIClickTheButton(string buttonText)
        {
            // Find and click the button with the specified text
            _errorPage.ClickHomeButton();
            
            // Wait briefly for the navigation to happen
            System.Threading.Thread.Sleep(1000);
        }
        
        [When(@"I see the ""Contact Support"" button")]
        public void WhenISeeTheContactSupportButton()
        {
            // Find the button with the specified text
            bool supportButtonExists = _driver.FindElements(By.CssSelector(".bi-headset")).Count > 0;
            
            // Wait briefly for the navigation to happen
            System.Threading.Thread.Sleep(1000);
        }

        [Then(@"I should be redirected to the home page")]
        public void ThenIShouldBeRedirectedToTheHomePage()
        {
            // Verify we're on the home page
            Assert.That(_homePage.IsPageLoaded(), Is.True, "Home page failed to load after clicking home button");
        }
        
        [Then(@"I should verify that the contact support button exists on the error page")]
        public void ThenIShouldVerifyThatTheContactSupportButtonExistsOnTheErrorPage()
        {
            bool supportButtonExists = _driver.FindElements(By.CssSelector(".bi-headset")).Count > 0;
            // Verify we're on the contact support page
            Assert.That(_errorPage.IsPageLoaded() && supportButtonExists, Is.True, "Contact support page failed to load after clicking contact support button");
        }

        public void Dispose()
        {
            // Release the driver instance when this step definition is disposed
            WebDriverSingleton.ReleaseInstance();
        }
    }
}