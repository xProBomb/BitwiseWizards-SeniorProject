using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Reqnroll;
using System;
using System.Linq;
using System.Threading;
using OpenQA.Selenium.Support.UI;

[Binding]
public class SearchSteps : IDisposable
{
    private IWebDriver _driver;
    private WebDriverWait _wait;
    private readonly string _baseUrl = "http://localhost:5102"; 

    public SearchSteps()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--window-size=1920,1080");

        _driver = new ChromeDriver(options);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20)); 
    }

    [Given(@"I am on the search page")]
    public void GivenIAmOnTheSearchPage()
    {
        _driver.Navigate().GoToUrl($"{_baseUrl}/Search");
        _wait.Until(d => d.FindElement(By.Id("searchTerm")));
    }

    [When(@"I select ""(.*)"" from the dropdown")]
    public void WhenISelectFromTheDropdown(string type)
    {
        var dropdown = new SelectElement(_driver.FindElement(By.Id("searchType")));
        dropdown.SelectByValue(type);
    }

    [When(@"I type ""(.*)"" into the search bar")]
public void WhenITypeIntoTheSearchBar(string query)
{
    var input = _driver.FindElement(By.Id("searchTerm"));
    input.Clear();
    input.SendKeys(query);

    // Trigger search by clicking the magnifying glass button
    var searchBtn = _driver.FindElement(By.CssSelector("button.input-group-text"));
    searchBtn.Click();
}


    [When(@"I wait for results to appear")]
    public void WhenIWaitForResultsToAppear()
    {
        _wait.Until(driver =>
        {
            var results = driver.FindElement(By.Id("searchResults"));
            return results.Text.Length > 0;
        });
    }

    [Then(@"I should see user cards in the results")]
    public void ThenIShouldSeeUserCardsInTheResults()
    {
        var cards = _driver.FindElements(By.CssSelector(".user-card"));
        Assert.That(cards.Count, Is.GreaterThan(0), "No user cards found");
    }

    [Then(@"I should see post cards in the results")]
        public void ThenIShouldSeePostCardsInTheResults()
        {
            // Wait for the container to appear and have content
            var container = _wait.Until(driver => driver.FindElement(By.Id("searchResults")));

            // Optional log of the container's HTML before waiting
            Console.WriteLine("Initial container HTML:\n" + container.GetAttribute("outerHTML"));

            // Wait until at least one post-card exists
            _wait.Until(driver => driver.FindElements(By.CssSelector(".post-card")).Any());

            var cards = _driver.FindElements(By.CssSelector(".post-card"));

            // Log the final state
            Console.WriteLine("Final container HTML:\n" + container.GetAttribute("outerHTML"));
            Console.WriteLine("Found " + cards.Count + " .post-card(s)");

            Assert.That(cards.Count, Is.GreaterThan(0), "No post cards found");
        }


    [When(@"I click the ""View Profile"" button for the first result")]
    public void WhenIClickViewProfile()
    {
        var button = _driver.FindElement(By.CssSelector(".user-card .view-profile-btn"));
        button.Click();
    }

    [Then(@"I should be taken to the user's profile page")]
    public void ThenIShouldBeOnUserProfilePage()
    {
        _wait.Until(driver =>
        {
            var url = driver.Url;
            return url.Contains("/Profile/User/");
        });

        Assert.That(_driver.Url, Does.Contain("/Profile/User/"));
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }
}
