using OpenQA.Selenium;
using Reqnroll;
using NUnit.Framework;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using TestTrustTrade.Reqnroll.Pages;
using TestTrustTrade.Reqnroll.Services;

namespace TestTrustTrade.Reqnroll.StepDefinitions
{
    [Binding]
    public class SearchSteps : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private readonly SearchPage _searchPage;

        public SearchSteps()
        {
            _driver = WebDriverSingleton.Instance;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
            _searchPage = new SearchPage();
        }

        [Given(@"I am on the search page")]
        public void GivenIAmOnTheSearchPage()
        {
            _searchPage.NavigateTo();
            Assert.That(_searchPage.IsLoaded(), Is.True, "Search page failed to load");
        }

        [When(@"I select ""(.*)"" from the dropdown")]
        public void WhenISelectFromTheDropdown(string type)
        {
            bool selected = _searchPage.SelectSearchType(type);
            Assert.That(selected, Is.True, $"Failed to select search type '{type}'");
        }

        [When(@"I type ""(.*)"" into the search bar")]
        public void WhenITypeIntoTheSearchBar(string query)
        {
            _searchPage.TypeSearchQuery(query);
        }

        [When(@"I wait for results to appear")]
        public void WhenIWaitForResultsToAppear()
        {
            Assert.That(_searchPage.WaitForResults(), Is.True, "Search results did not appear in time");
        }

        [Then(@"I should see user cards in the results")]
        public void ThenIShouldSeeUserCardsInTheResults()
        {
            int count = _searchPage.CountUserCards();
            Assert.That(count, Is.GreaterThan(0), "No user cards found");
        }

        [Then(@"I should see post cards in the results")]
        public void ThenIShouldSeePostCardsInTheResults()
        {
            int count = _searchPage.CountPostCards();
            Assert.That(count, Is.GreaterThan(0), "No post cards found");
        }

        [When(@"I click the ""View Profile"" button for the first result")]
        public void WhenIClickViewProfile()
        {
            Assert.That(_searchPage.ClickFirstProfileButton(), Is.True, "Failed to click 'View Profile' button");
        }

        [Then(@"I should be taken to the user's profile page")]
        public void ThenIShouldBeOnUserProfilePage()
        {
            Assert.That(_searchPage.IsOnUserProfilePage(), Is.True, "User profile page was not reached");
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
