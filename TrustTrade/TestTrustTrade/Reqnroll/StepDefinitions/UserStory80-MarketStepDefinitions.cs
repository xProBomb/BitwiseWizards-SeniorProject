using NUnit.Framework;
using Reqnroll;
using System.Collections.Generic;
using System.Linq;
using TrustTrade.ViewModels;

namespace TrustTrade.Tests.StepDefinitions
{
    [Binding, Scope(Feature = "UserStory80-Market")]
    public class MarketSteps
    {
        private List<StockViewModel> _allStocks;
        private List<StockViewModel> _allCrypto;
        private List<StockViewModel> _filtered;
        private string _viewMode;

        [Given(@"I have a list of stocks including ""(.*)"", ""(.*)"", and ""(.*)""")]
        public void GivenIHaveAListOfStocks(string t1, string t2, string t3)
        {
            _allStocks = new List<StockViewModel>
            {
                new StockViewModel { Ticker = t1 },
                new StockViewModel { Ticker = t2 },
                new StockViewModel { Ticker = t3 }
            };
        }

        [When(@"I filter the stocks with the query ""(.*)""")]
        public void WhenIFilterWithQuery(string query)
        {
            _filtered = _allStocks
                .Where(s => s.Ticker.Contains(query, System.StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        [Then(@"the result should include ""(.*)"" only")]
        public void ThenTheResultShouldIncludeOnly(string expected)
        {
            Assert.That(_filtered.Count, Is.EqualTo(1));
            Assert.That(_filtered[0].Ticker, Is.EqualTo(expected));
        }

        [Given(@"I am on the market page with stocks and crypto")]
        public void GivenIAmOnMarketPageWithStocksAndCrypto()
        {
            _allStocks = new List<StockViewModel>
            {
                new StockViewModel { Ticker = "AAPL", Name = "Apple" }
            };

            _allCrypto = new List<StockViewModel>
            {
                new StockViewModel { Ticker = "BTC", Name = "Bitcoin" }
            };
        }

        [When(@"I select the ""(.*)"" view")]
        public void WhenISelectTheView(string type)
        {
            _viewMode = type.ToLower();
        }

        [Then(@"I should see a list that includes ""(.*)"" and not ""(.*)""")]
        public void ThenIShouldSeeAListThatIncludesAndNot(string include, string exclude)
        {
            var list = _viewMode == "crypto" ? _allCrypto : _allStocks;

            Assert.That(list.Any(i => i.Ticker == include), $"Expected to include {include}");
            Assert.That(list.All(i => i.Ticker != exclude), $"Expected not to include {exclude}");
        }
    }
}
