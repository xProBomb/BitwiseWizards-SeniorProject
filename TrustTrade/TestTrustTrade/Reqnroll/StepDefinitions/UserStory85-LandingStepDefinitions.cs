using OpenQA.Selenium;
using Reqnroll;
using TestTrustTrade.Reqnroll.Pages;
using TestTrustTrade.Reqnroll.Services;

namespace TestTrustTrade.Reqnroll.StepDefinitions;

[Binding]
public class UserStory85_LandingStepDefinitions : IDisposable
{
    private readonly LandingPage _landingPage;
        
    // Store the current WebDriver reference for easier access
    private readonly IWebDriver _driver;

    public UserStory85_LandingStepDefinitions()
    {
        // Get driver reference from the singleton
        _driver = WebDriverSingleton.Instance;
            
        // Initialize the landing page
        _landingPage = new LandingPage();
    }

    [Given(@"I am on the landing page")]
    public void GivenIAmOnTheLandingPage()
    {
        // Use the driver to navigate to the landing page
        bool success = _landingPage.IsPageLoaded();
            
        // Verify we're logged in
        Assert.That(success, Is.True, "Failed to navigate to the landing page");
    }
    
    [When(@"I leave the landing page")]
    public void WhenILeaveTheLandingPage()
    {
        // Use the driver to click to the index page
        _landingPage.EnterSite();
            
        // Wait for the page to load
        Thread.Sleep(2000);
        
        bool success = _landingPage.IsWidgetTitleVisible();
        
        // Verify we're logged in
        Assert.That(success, Is.True, "Failed to navigate to the Index page");
    }
    
    [Then(@"I should be taken to the Index page")]
    public void ThenIShouldBeTakenToTheIndexPage()
    {
        bool success = _landingPage.IsWidgetTitleVisible();
        
        // Verify we're logged in
        Assert.That(success, Is.True, "Failed to navigate to the Index page");
    }
    
    public void Dispose()
    {
        // Release the driver instance when this step definition is disposed
        // but don't actually quit the driver since other tests might use it
        WebDriverSingleton.ReleaseInstance();
    }
        
    [AfterTestRun]
    public static void CleanUp()
    {
        // Force quit the driver after all tests are complete
        WebDriverSingleton.ForceQuit();
    }
}