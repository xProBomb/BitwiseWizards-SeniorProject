using OpenQA.Selenium;
using Reqnroll;
using TestTrustTrade.Reqnroll.Pages;
using TestTrustTrade.Reqnroll.Services;

namespace TestTrustTrade.Reqnroll.StepDefinitions
{
    [Binding]
    public class ProfileBackgroundStepDefinitions : IDisposable
    {
        private readonly LoginService _loginService;
        private readonly ProfileBackgroundPage _profilePage;
        private readonly IWebDriver _driver;

        public ProfileBackgroundStepDefinitions()
        {
            _driver = WebDriverSingleton.Instance;
            _loginService = new LoginService();
            _profilePage = new ProfileBackgroundPage();
        }

        // Note: "User1 is logged in" step is already defined in NotificationSteps

        [When(@"User1 navigates to their profile")]
        public void WhenUser1NavigatesToTheirProfile()
        {
            _profilePage.NavigateTo();
            Assert.That(_profilePage.IsPageLoaded(), Is.True, "Profile page failed to load");
        }

        [When(@"User1 adds a space cat background image")]
        public void WhenUser1AddsASpaceCatBackgroundImage()
        {
            bool success = _profilePage.AddSpaceCatBackground();
            Assert.That(success, Is.True, "Failed to add space cat background image");
        }

        [Then(@"the profile should display the background image")]
        public void ThenTheProfileShouldDisplayTheBackgroundImage()
        {
            bool hasBackground = _profilePage.HasBackgroundImage();
            Assert.That(hasBackground, Is.True, "Background image was not displayed");
        }

        [When(@"User1 removes the background image")]
        public void WhenUser1RemovesTheBackgroundImage()
        {
            bool success = _profilePage.RemoveBackgroundImage();
            Assert.That(success, Is.True, "Failed to remove background image");
        }

        [Then(@"the profile should not display any background image")]
        public void ThenTheProfileShouldNotDisplayAnyBackgroundImage()
        {
            bool hasBackground = _profilePage.HasBackgroundImage();
            Assert.That(hasBackground, Is.False, "Background image is still displayed");
        }

        public void Dispose()
        {
            WebDriverSingleton.ReleaseInstance();
        }
    }
}