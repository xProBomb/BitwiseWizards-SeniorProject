using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using NUnit.Framework;
using Reqnroll;

namespace TrustTrade.Tests.StepDefinitions;

[Binding]
public class MarketModalSteps
{
    private IWebDriver _driver;

    [BeforeScenario]
    public void Setup()
    {
        _driver = new ChromeDriver();
    }

    [AfterScenario]
    public void TearDown()
    {
        _driver?.Quit();
    }

    [Given("I am on the market page")]
    public void GivenIAmOnTheMarketPage()
    {
        _driver.Navigate().GoToUrl("http://localhost:5102/Market"); 
    }

    [When("I click on a stock card")]
    public void WhenIClickOnAStockCard()
    {
        var card = _driver.FindElement(By.ClassName("stock-card"));
        card.Click();
        Thread.Sleep(1000); 
    }

    [Then("the stock modal should be visible")]
    public void ThenTheStockModalShouldBeVisible()
    {
        var modal = _driver.FindElement(By.Id("stockModal"));
        Assert.That(modal.Displayed);
    }
}
