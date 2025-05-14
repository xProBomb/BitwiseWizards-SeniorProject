using OpenQA.Selenium;
using Reqnroll;
using System;
using System.IO;
using System.Linq;
using TestTrustTrade.Reqnroll.Pages;
using TestTrustTrade.Reqnroll.Services;

namespace TestTrustTrade.Reqnroll.StepDefinitions
{
    [Binding]
    public class PostImagesStepDefinitions : IDisposable
    {
        private readonly LoginService _loginService;
        private readonly CreatePostPage _createPage;
        private readonly IWebDriver _driver;

        public PostImagesStepDefinitions()
        {
            _driver = WebDriverSingleton.Instance;
            _loginService = new LoginService();
            _createPage = new CreatePostPage();
        }

        // Note: "User1 is logged in" step is already defined in NotificationSteps

        [When(@"User1 navigates to post creation")]
        public void WhenUser1NavigatesToPostCreation()
        {
            _createPage.NavigateTo();
            if (!_createPage.IsPageLoaded())
                throw new Exception("Create Post page did not load.");
        }

        [When(@"User1 adds an image to post")]
        public void WhenUser1AddsAnImageToPost()
        {
            var imagePath = GetTestDataPath("sample.png");
            _createPage.EnterTitle("Image Test Post");
            _createPage.EnterContent("This post contains an image.");
            _createPage.UploadImage(imagePath);
        }

        [Then(@"the post should display image")]
        public void ThenThePostShouldDisplayImage()
        {
            _createPage.SubmitForm();

            // go to the home page to see the post
            _driver.Navigate().GoToUrl("http://localhost:5102/");

            // Assert image is present
            var images = _driver.FindElements(By.CssSelector(".post-images"));
            if (images.Count == 0)
                throw new Exception("No images were found in the created post.");
        }

        public void Dispose()
        {
            WebDriverSingleton.ReleaseInstance();
        }

        private static string GetTestDataPath(string fileName)
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;

            while (dir != null && !Directory.GetDirectories(dir).Any(d => Path.GetFileName(d) == "TestTrustTrade"))
            {
                dir = Directory.GetParent(dir)?.FullName;
            }

            if (dir == null)
                throw new DirectoryNotFoundException("TestTrustTrade directory not found.");

            var fullPath = Path.Combine(dir, "TestTrustTrade", "TestData", fileName);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Test file not found: {fullPath}");

            return fullPath;
        }

    }
}
