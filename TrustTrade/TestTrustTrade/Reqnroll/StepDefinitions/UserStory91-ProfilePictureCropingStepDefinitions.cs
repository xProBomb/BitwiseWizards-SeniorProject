using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.IO;
using Reqnroll;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace TestTrustTrade.StepDefinitions
{
    [Binding]
    public class ProfilePictureStepDefinitions : IDisposable
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;

        [BeforeScenario]
        public void Setup()
        {
            // Use standard Chrome instead of headless for testing
            _driver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            // Navigate to the base URL
            _driver.Navigate().GoToUrl("http://localhost:5102/");
            _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);
        }

        [AfterScenario]
        public void Teardown()
        {
            _driver.Quit();
        }

        public void Dispose()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }

        private void Login()
        {
            _driver.Navigate().GoToUrl("http://localhost:5102/Identity/Account/Login");

            var emailField = _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Input_Email")));
            var passwordField = _driver.FindElement(By.Id("password-field"));
            var loginButton = _driver.FindElement(By.Id("login-submit"));

            emailField.SendKeys("testuser@gmail.com");
            passwordField.SendKeys("Test#123"); // Replace with a valid password
            loginButton.Click();

            // Wait for redirection to the home page
            _wait.Until(ExpectedConditions.UrlContains("http://localhost:5102/"));
        }

        [Given(@"I am on the Profile page")]
        public void GivenIAmOnTheProfilePage()
        {
            Login(); // Make sure we're logged in first
            _driver.Navigate().GoToUrl("http://localhost:5102/Profile");
            
            // Simply wait for the page to load, don't wait for specific elements yet
            Thread.Sleep(2000);
        }

        [When(@"I click the ""Change"" button")]
        public void WhenIClickTheChangeButton()
        {
            var button = _driver.FindElement(By.XPath("//button[contains(text(), 'Change')]"));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", button);
        }

        [When(@"I click the ""Crop & Upload"" button")]
        public void WhenIClickTheCropAndUploadButton()
        {
            var cropAndUploadButton = _driver.FindElement(By.Id("cropAndUploadButton"));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", cropAndUploadButton);
        }

        [When(@"I upload a valid image file")]
        public void WhenIUploadAValidImageFile()
        {
            var fileInput = _driver.FindElement(By.Id("profilePictureInput"));
            var tempFilePath = Path.Combine(Path.GetTempPath(), "dummy-image.Jpeg");

            try
            {
                // Create a dummy image file
                using (var surface = SkiaSharp.SKSurface.Create(new SkiaSharp.SKImageInfo(150, 150)))
                {
                    var canvas = surface.Canvas;
                    canvas.Clear(SkiaSharp.SKColors.Blue); // Fill with a solid color
                    using (var image = surface.Snapshot())
                    using (var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Jpeg, 100))
                    {
                        using (var stream = File.OpenWrite(tempFilePath))
                        {
                            data.SaveTo(stream);
                        }
                    }
                }

                fileInput.SendKeys(tempFilePath);
            }
            finally
            {
                // Clean up the temporary file
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }

        [When(@"I upload an invalid image file")]
        public void WhenIUploadAnInvalidImageFile()
        {
            var fileInput = _driver.FindElement(By.Id("profilePictureInput"));
            var tempInvalidFilePath = Path.Combine(Path.GetTempPath(), "invalid-file.txt");

            try
            {
                // Create a dummy invalid file
                File.WriteAllText(tempInvalidFilePath, "This is not an image file.");
                fileInput.SendKeys(tempInvalidFilePath);
            }
            finally
            {
                // Clean up the temporary file
                if (File.Exists(tempInvalidFilePath))
                {
                    File.Delete(tempInvalidFilePath);
                }
            }
        }

        [When(@"I crop the image using the cropper tool")]
        public void WhenICropTheImageUsingTheCropperTool()
        {
            _wait.Until(ExpectedConditions.ElementExists(By.Id("cropperImage")));
        }

        [Then(@"the profile picture should be updated successfully")]
        public void ThenTheProfilePictureShouldBeUpdatedSuccessfully()
        {
            // ensure you are on the profile page
            _driver.Navigate().GoToUrl("http://localhost:5102/Profile");
            _wait.Until(ExpectedConditions.ElementExists(By.Id("profile-image")));

            var profilePicture = _driver.FindElement(By.Id("profile-image"));
            Assert.That(profilePicture.GetAttribute("src"), Does.Contain("data:image/jpeg;base64,"));
        }

        [Then(@"I should see an error message indicating an invalid file type")]
        public void ThenIShouldSeeAnErrorMessageIndicatingAnInvalidFileType()
        {
            _driver.Navigate().GoToUrl("http://localhost:5102/Profile");
            var errorMessage = _driver.FindElement(By.ClassName("text-danger"));
            Assert.That(errorMessage.Displayed, Is.True, "Error message is not displayed as expected.");
        }
    }
}