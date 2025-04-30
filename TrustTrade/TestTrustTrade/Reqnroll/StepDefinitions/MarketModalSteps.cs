using OpenQA.Selenium;
using Reqnroll;
using NUnit.Framework;
using TestTrustTrade.Reqnroll.Services;
using TestTrustTrade.Reqnroll.Pages;

namespace TestTrustTrade.Reqnroll.StepDefinitions
{
    [Binding]
    public class MarketModalSteps : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly MarketPage _marketPage;

        public MarketModalSteps()
        {
            _driver = WebDriverSingleton.Instance;
            _marketPage = new MarketPage();
        }

        [Given(@"I am on the market page with modal")]
        public void GivenIAmOnTheModalMarketPage()
        {
            _marketPage.NavigateTo();
            Assert.That(_marketPage.IsPageLoaded(), Is.True, "Market page failed to load");
        }

        [When(@"I click on a stock card")]
        public void WhenIClickOnAStockCard()
        {
            bool clickSuccess = _marketPage.ClickFirstStockCard();
            Assert.That(clickSuccess, Is.True, "Failed to click on a stock card");
        }

        [Then(@"the stock modal should be visible")]
        public void ThenTheStockModalShouldBeVisible()
        {
            bool isVisible = _marketPage.IsModalVisible();
            Assert.That(isVisible, Is.True, "Stock modal is not visible");
        }

        [Then(@"I should see history filter options")]
        public void ThenIShouldSeeHistoryFilterOptions()
        {
            var filters = _marketPage.GetHistoryFilterButtons();
            Assert.That(filters.Count, Is.EqualTo(3), "Expected 3 history filter buttons (3D, 5D, 7D)");
        }

        [When(@"I click the '(.*)' history filter")]
        public void WhenIClickTheHistoryFilter(string daysLabel)
        {
            bool clicked = _marketPage.ClickHistoryFilter(daysLabel);
            Assert.That(clicked, Is.True, $"Failed to click on {daysLabel} history filter button");
        }

        [Then(@"the '(.*)' history filter should be active")]
        public void ThenTheHistoryFilterShouldBeActive(string daysLabel)
        {
            string active = _marketPage.GetActiveHistoryFilter();
            Assert.That(active, Is.EqualTo(daysLabel), $"Expected '{daysLabel}' filter to be active, but found '{active}'");
        }

        public void Dispose()
        {
            WebDriverSingleton.ReleaseInstance();
        }

        [AfterTestRun]
        public static void CleanUp()
        {
            WebDriverSingleton.ForceQuit();
        }
    }
}
