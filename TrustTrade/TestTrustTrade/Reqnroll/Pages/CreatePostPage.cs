using OpenQA.Selenium;
using System;
using System.IO;

namespace TestTrustTrade.Reqnroll.Pages
{
    public class CreatePostPage : BasePage
    {
        // Locators
        private readonly By _titleInput = By.Id("Title");
        private readonly By _contentInput = By.Id("Content");
        private readonly By _isPublicCheckbox = By.Id("IsPublic");
        private readonly By _tagCheckboxes = By.Name("SelectedTags");
        private readonly By _imageInput = By.Id("imageInput");
        private readonly By _addImageButton = By.Id("addImageButton");
        private readonly By _submitButton = By.Id("submitButton");
        private readonly By _form = By.TagName("form");

        protected override string PageUrl => $"/Posts/Create";


        public override bool IsPageLoaded()
        {
            try
            {
                return WaitForElementVisible(_titleInput) != null && Driver.Url.Contains("/Posts/Create");
            }
            catch
            {
                return false;
            }
        }

        public void EnterTitle(string title)
        {
            var titleField = WaitForElementVisible(_titleInput);
            titleField.Clear();
            titleField.SendKeys(title);
        }

        public void EnterContent(string content)
        {
            var contentField = WaitForElementVisible(_contentInput);
            contentField.Clear();
            contentField.SendKeys(content);
        }

        public void SetIsPublic(bool isPublic)
        {
            var checkbox = WaitForElementClickable(_isPublicCheckbox);
            if (checkbox.Selected != isPublic)
            {
                checkbox.Click();
            }
        }

        public void SelectTag(string tagName)
        {
            var checkboxes = Driver.FindElements(_tagCheckboxes);
            foreach (var checkbox in checkboxes)
            {
                if (checkbox.GetAttribute("value").Equals(tagName, StringComparison.OrdinalIgnoreCase))
                {
                    if (!checkbox.Selected)
                    {
                        checkbox.Click();
                    }
                    return;
                }
            }

            throw new Exception($"Tag '{tagName}' not found.");
        }

        public void UploadImage(string filePath)
        {
            // Ensure the file exists
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Image file not found", filePath);

            var input = WaitForElementVisible(_imageInput);
            input.SendKeys(filePath);

            SafeClick(_addImageButton);

            System.Threading.Thread.Sleep(500); // allow time for JS to handle Base64 preview
        }

        public void SubmitForm()
        {
        }
    }
}
