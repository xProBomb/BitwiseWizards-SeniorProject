using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace TestTrustTrade.Reqnroll.Pages
{
    public class SearchPage : BasePage
    {
        protected override string PageUrl => "/Search";

        public override bool IsPageLoaded()
        {
            try
            {
                return WaitForElementVisible(By.Id("searchTerm")).Displayed;
            }
            catch
            {
                return false;
            }
        }

        public bool SelectSearchType(string value)
        {
            try
            {
                var dropdown = new SelectElement(WaitForElementVisible(By.Id("searchType")));
                dropdown.SelectByValue(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void TypeSearchQuery(string query)
        {
            var input = WaitForElementVisible(By.Id("searchTerm"));
            input.Clear();
            input.SendKeys(query);
            SafeClick(By.CssSelector("button.input-group-text"));
        }

        public bool WaitForResults()
        {
            try
            {
                return Wait.Until(driver =>
                {
                    var container = driver.FindElement(By.Id("searchResults"));
                    return container.Text.Length > 0;
                });
            }
            catch
            {
                return false;
            }
        }

        public int CountUserCards()
        {
            return Driver.FindElements(By.CssSelector(".user-card")).Count;
        }

        public int CountPostCards()
        {
            try
            {
                Wait.Until(driver => driver.FindElements(By.CssSelector(".post-card")).Any());
                return Driver.FindElements(By.CssSelector(".post-card")).Count;
            }
            catch
            {
                return 0;
            }
        }

        public bool ClickFirstProfileButton()
        {
            try
            {
                SafeClick(By.CssSelector(".user-card .view-profile-btn"));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool IsOnUserProfilePage()
        {
            try
            {
                return Wait.Until(driver => driver.Url.Contains("/Profile/User/"));
            }
            catch
            {
                return false;
            }
        }
    }
}
