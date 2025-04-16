using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Reqnroll;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrustTrade.Controllers;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;

namespace TestTrustTrade.Reqnroll.StepDefinitions
{
    [Binding]
    public class FinancialNewsStepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;
        private Mock<IFinancialNewsService> _newsServiceMock;
        private Mock<ILogger<FinancialNewsController>> _loggerMock;
        private FinancialNewsController _controller;
        private IActionResult _result;
        private List<FinancialNewsItem> _newsItems;
        private string _category;
        private int _count;
        private Exception _caughtException;

        public FinancialNewsStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _newsServiceMock = new Mock<IFinancialNewsService>();
            _loggerMock = new Mock<ILogger<FinancialNewsController>>();
            _newsItems = new List<FinancialNewsItem>();
        }

        [Given(@"I have a financial news controller")]
        public void GivenIHaveAFinancialNewsController()
        {
            _controller = new FinancialNewsController(_newsServiceMock.Object, _loggerMock.Object);
        }

        [Given(@"there are (.*) news items in the database")]
        public void GivenThereAreNewsItemsInTheDatabase(int count)
        {
            _newsItems.Clear();
            for (int i = 1; i <= count; i++)
            {
                _newsItems.Add(new FinancialNewsItem
                {
                    Id = i,
                    Title = $"Test News {i}",
                    Summary = $"Test Summary {i}",
                    Url = $"https://example.com/news/{i}",
                    Source = "Test Source",
                    Category = i % 2 == 0 ? "Stock" : "Crypto",
                    TimePublished = DateTime.UtcNow.AddHours(-i),
                    IsActive = true
                });
            }

            _newsServiceMock
                .Setup(s => s.GetLatestNewsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((string category, int count) => 
                {
                    var filteredItems = string.IsNullOrEmpty(category) 
                        ? _newsItems 
                        : _newsItems.Where(n => n.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
                    
                    return filteredItems.Take(count).ToList();
                });
        }

        [Given(@"I specify the category ""(.*)""")]
        public void GivenISpecifyTheCategory(string category)
        {
            _category = category;
        }

        [Given(@"I want to see (.*) items")]
        public void GivenIWantToSeeItems(int count)
        {
            _count = count;
        }

        [When(@"I request the financial news index page")]
        public async Task WhenIRequestTheFinancialNewsIndexPage()
        {
            try
            {
                _result = await _controller.Index(_category);
            }
            catch (Exception ex)
            {
                _caughtException = ex;
            }
        }

        [When(@"I request the financial news widget")]
        public async Task WhenIRequestTheFinancialNewsWidget()
        {
            try
            {
                _result = await _controller.NewsWidget(_category, _count);
            }
            catch (Exception ex)
            {
                _caughtException = ex;
            }
        }

        [When(@"the service throws an exception")]
        public void WhenTheServiceThrowsAnException()
        {
            _newsServiceMock
                .Setup(s => s.GetLatestNewsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("Test exception"));
        }

        [Then(@"the result should be a view")]
        public void ThenTheResultShouldBeAView()
        {
            Assert.That(_result, Is.InstanceOf<ViewResult>());
        }

        [Then(@"the result should be a partial view")]
        public void ThenTheResultShouldBeAPartialView()
        {
            Assert.That(_result, Is.InstanceOf<PartialViewResult>());
        }

        [Then(@"the view model should contain (.*) news items")]
        public void ThenTheViewModelShouldContainNewsItems(int expectedCount)
        {
            if (_result is ViewResult viewResult)
            {
                var model = viewResult.Model as IEnumerable<FinancialNewsItem>;
                Assert.That(model, Is.Not.Null);
                Assert.That(model.Count(), Is.EqualTo(expectedCount));
            }
            else if (_result is PartialViewResult partialViewResult)
            {
                var model = partialViewResult.Model as IEnumerable<FinancialNewsItem>;
                Assert.That(model, Is.Not.Null);
                Assert.That(model.Count(), Is.EqualTo(expectedCount));
            }
        }

        [Then(@"the view name should be ""(.*)""")]
        public void ThenTheViewNameShouldBe(string viewName)
        {
            if (_result is ViewResult viewResult)
            {
                Assert.That(viewResult.ViewName, Is.EqualTo(viewName));
            }
            else if (_result is PartialViewResult partialViewResult)
            {
                Assert.That(partialViewResult.ViewName, Is.EqualTo(viewName));
            }
        }

        [Then(@"the result should be an error view")]
        public void ThenTheResultShouldBeAnErrorView()
        {
            Assert.That(_result, Is.InstanceOf<ViewResult>());
            var viewResult = _result as ViewResult;
            Assert.That(viewResult.ViewName, Is.EqualTo("Error"));
        }

        [Then(@"the result should be an error message")]
        public void ThenTheResultShouldBeAnErrorMessage()
        {
            Assert.That(_result, Is.InstanceOf<ContentResult>());
            var contentResult = _result as ContentResult;
            Assert.That(contentResult.Content, Does.Contain("Unable to load financial news"));
        }

        [Then(@"the category in ViewBag should be ""(.*)""")]
        public void ThenTheCategoryInViewBagShouldBe(string category)
        {
            if (_result is ViewResult viewResult)
            {
                Assert.That(viewResult.ViewData["CurrentCategory"], Is.EqualTo(category));
            }
            else if (_result is PartialViewResult partialViewResult)
            {
                Assert.That(partialViewResult.ViewData["CurrentCategory"], Is.EqualTo(category));
            }
        }
    }
}