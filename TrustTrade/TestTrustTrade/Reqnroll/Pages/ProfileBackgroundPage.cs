using OpenQA.Selenium;
using System;
using System.Threading;

namespace TestTrustTrade.Reqnroll.Pages
{
    public class ProfileBackgroundPage : BasePage
    {
        // Locators
        private readonly By _addBackgroundBtn = By.CssSelector("button[data-bs-toggle='modal'][data-bs-target='#updateBackgroundImageModal']");
        private readonly By _imageUrlBtn = By.CssSelector(".bi-link-45deg");
        private readonly By _backgroundUrlInput = By.CssSelector("#backgroundImageUrl");
        private readonly By _saveBackgroundBtn = By.CssSelector("#saveCropBtn");
        private readonly By _removeBackgroundBtn = By.CssSelector(".bi-trash");
        private readonly By _notCroppedData = By.CssSelector("#includeCroppedData"); 
        private readonly By _profileHeader = By.CssSelector(".profile-header");
        private readonly By _backgroundElement = By.CssSelector(".profile-background");
        
        // Constant image URL
        private const string SpaceCatUrl = "https://media1.tenor.com/m/2roX3uxz_68AAAAC/cat-space.gif";
        
        protected override string PageUrl => "/Profile";
        
        public override bool IsPageLoaded()
        {
            try
            {
                return WaitForElementVisible(_profileHeader) != null;
            }
            catch
            {
                return false;
            }
        }
        
        public bool AddSpaceCatBackground()
        {
            try
            {
                // Click "Add Background" button to open modal
                SafeClick(_addBackgroundBtn);
                
                // Wait for modal to appear
                Thread.Sleep(1000);
                
                // Click "Image Url" button/option
                var imageUrlButton = WaitForElementClickable(_imageUrlBtn);
                imageUrlButton.Click();
                
                // Enter the space cat URL
                var urlInput = WaitForElementVisible(_backgroundUrlInput);
                urlInput.Clear();
                urlInput.SendKeys(SpaceCatUrl);
                
                // Uncheck "Include cropped data"
                var notCroppedDataCheckbox = WaitForElementClickable(_notCroppedData);
                notCroppedDataCheckbox.Click();
                
                // Click "Save Background" button
                SafeClick(_saveBackgroundBtn);
                
                // Wait for the action to complete and modal to close
                Thread.Sleep(2000);
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to add background image: {ex.Message}");
                return false;
            }
        }
        
        public bool HasBackgroundImage()
        {
            try
            {
                // Check if background element exists
                var backgroundElements = Driver.FindElements(_backgroundElement);
                return backgroundElements.Count > 0;
            }
            catch
            {
                return false;
            }
        }
        
        public bool RemoveBackgroundImage()
        {
            try
            {
                // Click the remove button
                SafeClick(_removeBackgroundBtn);
                
                // Wait for the action to complete
                Thread.Sleep(2000);
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to remove background image: {ex.Message}");
                return false;
            }
        }
    }
}