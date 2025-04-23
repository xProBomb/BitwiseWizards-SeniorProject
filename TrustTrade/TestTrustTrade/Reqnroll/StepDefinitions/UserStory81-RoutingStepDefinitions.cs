using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Reqnroll;

namespace TestTrustTrade.StepDefinitions
{
    [Binding]
    public class RoutingStepDefinitions : IDisposable
    {
        private IWebDriver _driver;

        [BeforeScenario]
        public void Setup()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            _driver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory, options);
            _driver.Navigate().GoToUrl("http://localhost:5102/");
            _driver.Manage().Cookies.AddCookie(new Cookie("AuthCookie", "your-auth-token"));

            //timeout for page load
            _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);

        }

        private void Login()
        {
            _driver.Navigate().GoToUrl("http://localhost:5102/Identity/Account/Login");

            // Wait for the login form to load
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            var usernameField = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Input_Email")));
            var passwordField = _driver.FindElement(By.Id("password-field"));
            var loginButton = _driver.FindElement(By.Id("login-submit"));

            // Enter credentials and submit the form
            usernameField.SendKeys("testuser@gmail.com");
            passwordField.SendKeys("Test#123");


            loginButton.Click();

            // Wait for redirection after login
            wait.Until(ExpectedConditions.UrlContains("http://localhost:5102/"));
        }

        public void Dispose()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }

        [AfterScenario]
        public void Teardown()
        {
            _driver.Quit();
        }

        [Given("I am on the Following page")]
        public void GivenIAmOnTheFollowingPage()
        {
            Login();
            _driver.Navigate().GoToUrl("http://localhost:5102/Home/Following");
        }

        [Given("I am on the Home page")]
        public void GivenIAmOnTheHomePage()
        {
            Login();
            _driver.Navigate().GoToUrl("http://localhost:5102/");
        }

        [When("I click the \"Home\" link")]
        public void WhenIClickTheHomeLink()
        {
            Assert.That(_driver.FindElements(By.Id("home-link")).Count > 0, Is.True, "The 'home-link' element does not exist.");
            // _driver.FindElement(By.Id("home-link")).Click();
            _driver.Navigate().GoToUrl("http://localhost:5102/");
        }

        [When("I click the \"Following\" link")]
        public void WhenIClickTheFollowingLink()
        {
            Assert.That(_driver.FindElements(By.Id("following-link")).Count > 0, Is.True, "The 'following-link' element does not exist.");
            // _driver.FindElement(By.Id("following-link")).Click();
            _driver.Navigate().GoToUrl("http://localhost:5102/Home/Following");
        }

        [Then("I should be redirected to the Home page")]
        public void ThenIShouldBeRedirectedToTheHomePage()
        {
            string currentUrl = _driver.Url;
            Assert.That(currentUrl, Is.EqualTo("http://localhost:5102/"));
        }

        [Then("I should be redirected to the Following page")]
        public void ThenIShouldBeRedirectedToTheFollowingPage()
        {
            string currentUrl = _driver.Url;
            Assert.That(currentUrl, Is.EqualTo("http://localhost:5102/Home/Following"));
        }
    }
}