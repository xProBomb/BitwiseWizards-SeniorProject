using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Reqnroll;

namespace TrustTrade.Tests.StepDefinitions;

[Binding]
public class UserPostsSteps : IDisposable
{
    private IWebDriver _driver;

    [BeforeScenario]
    public void Setup()
    {
        _driver = new ChromeDriver();
    }

    public void Dispose()
    {
        if (_driver != null)
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }

    [AfterScenario]
    public void Teardown()
    {
        _driver.Quit();
    }

    [Given("I am viewing the list of a person's posts")]
    public void GivenIAmViewingTheListOfAPersonsPosts()
    {
        _driver.Navigate().GoToUrl("http://localhost:5102/Profile/User/user1/Posts");
    }

    [When("I click on a post")]
    public void ThenIClickOnAPost()
    {
        var post = _driver.FindElement(By.ClassName("clickable-post"));
        post.Click();
    }

    [Then("I should be redirected to the post details page")]
    public void ThenIShouldBeRedirectedToThePostDetailsPage()
    {
        string currentUrl = _driver.Url;
        Assert.That(currentUrl, Is.EqualTo("http://localhost:5102/Posts/Details/1"));
    }
}