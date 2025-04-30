using OpenQA.Selenium;
using System.Collections.Generic;
using System.Linq;

namespace TestTrustTrade.Reqnroll.Pages
{
    public class MarketPage : BasePage
    {
        protected override string PageUrl => "/Market";

        public override bool IsPageLoaded()
        {
            try
            {
                return WaitForElementVisible(By.Id("marketCards")).Displayed;
            }
            catch
            {
                return false;
            }
        }

        public bool ClickFirstStockCard()
        {
            try
            {
                var card = WaitForElementVisible(By.ClassName("stock-card"));
                card.Click();
                System.Threading.Thread.Sleep(1000); // wait for modal animation
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool IsModalVisible()
        {
            try
            {
                var modal = WaitForElementVisible(By.Id("stockModal"));
                return modal.Displayed;
            }
            catch
            {
                return false;
            }
        }

        public IReadOnlyCollection<IWebElement> GetHistoryFilterButtons()
        {
            return Driver.FindElements(By.CssSelector("#historyFilter .chart-filter-button"));
        }

        public bool ClickHistoryFilter(string label)
        {
            try
            {
                var buttons = GetHistoryFilterButtons();
                var targetButton = buttons.FirstOrDefault(btn => btn.Text.Trim().Equals(label, System.StringComparison.OrdinalIgnoreCase));

                if (targetButton != null)
                {
                    targetButton.Click();
                    System.Threading.Thread.Sleep(300); // allow active class to switch
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public string GetActiveHistoryFilter()
        {
            try
            {
                var active = Driver.FindElement(By.CssSelector("#historyFilter .chart-filter-button.active"));
                return active.Text.Trim();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
