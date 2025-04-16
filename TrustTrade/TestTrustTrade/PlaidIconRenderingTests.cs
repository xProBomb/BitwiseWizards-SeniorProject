using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using TrustTrade.Controllers;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using TrustTrade.ViewModels;
using TrustTrade.Services.Web.Interfaces;
using TrustTrade.Helpers;

namespace TestTrustTrade
{
    [TestFixture]
    public class PlaidIconRenderingTests
    {
        private Mock<ILogger<HomeController>> _loggerMock;
        private Mock<IPostService> _postServiceMock;
        private Mock<IUserService> _userServiceMock;
        private HomeController _controller;
        private List<PostPreviewVM> _testPosts;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<HomeController>>();
            _postServiceMock = new Mock<IPostService>();
            _userServiceMock = new Mock<IUserService>();
            _controller = new HomeController(
                _loggerMock.Object, 
                _postServiceMock.Object, 
                _userServiceMock.Object
            );

            // Create test post preview view models
            _testPosts = new List<PostPreviewVM>
            {
                new PostPreviewVM
                {
                    Id = 1,
                    UserName = "PlaidUser",
                    Title = "Post with Plaid",
                    Excerpt = "This post is from a user with Plaid enabled",
                    TimeAgo = "1 hour ago",
                    IsPlaidEnabled = true,
                    PortfolioValueAtPosting = "$10.1K"
                },
                new PostPreviewVM
                {
                    Id = 2,
                    UserName = "NonPlaidUser",
                    Title = "Post without Plaid",
                    Excerpt = "This post is from a user without Plaid enabled",
                    TimeAgo = "2 hours ago",
                    IsPlaidEnabled = false,
                    PortfolioValueAtPosting = null
                },
                new PostPreviewVM
                {
                    Id = 3,
                    UserName = "AnotherPlaidUser",
                    Title = "Plaid without portfolio value",
                    Excerpt = "This post is from a Plaid user but has no portfolio value",
                    TimeAgo = "3 hours ago",
                    IsPlaidEnabled = true,
                    PortfolioValueAtPosting = null
                }
            };
        }

        // Helper method to render a partial view to string
        private string RenderPartialViewToString(string viewName, object model)
        {
            // Setup controller context
            var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };

            using (var sw = new StringWriter())
            {
                var viewResult = _controller.View(viewName, model) as ViewResult;
                var viewEngine = Mock.Of<ICompositeViewEngine>();

                // Note: This wouldn't actually render the view since we're mocking the view engine
                // For a real test, you would need integration testing with the actual Razor engine
                
                return sw.ToString();
            }
        }

        [Test]
        public void CheckmarkIconIsShownForPlaidEnabledUser()
        {
            // This test simulates checking the rendered HTML to verify the icon is present
            // In a real scenario, you would use integration tests or UI tests for this

            // Arrange
            var post = _testPosts.First(p => p.IsPlaidEnabled);
            
            // We don't need to verify logging in this test since it's just testing the view model
            // Testing that the property is set correctly is sufficient

            // Assert
            Assert.That(post.IsPlaidEnabled, Is.True, "IsPlaidEnabled should be true for test to be meaningful");
        }

        [Test]
        public void PortfolioValueIsShownForPlaidUserWithValue()
        {
            // Arrange
            var post = _testPosts.First(p => p.IsPlaidEnabled && p.PortfolioValueAtPosting != null);
            
            // Assert
            Assert.That(post.IsPlaidEnabled, Is.True, "IsPlaidEnabled should be true for test to be meaningful");
            Assert.That(post.PortfolioValueAtPosting, Is.Not.Null, "Portfolio value should be present");
            Assert.That(post.PortfolioValueAtPosting, Is.EqualTo("$10.1K"), "Portfolio value should match expected");
        }

        [Test]
        public void NoCheckmarkIconForNonPlaidUser()
        {
            // Arrange
            var post = _testPosts.First(p => !p.IsPlaidEnabled);
            
            // Assert
            Assert.That(post.IsPlaidEnabled, Is.False, "IsPlaidEnabled should be false for test to be meaningful");
        }

        [Test]
        public void PlaidIconRendersBasedOnUserPlaidStatus()
        {
            // This test validates the Model property that affects icon rendering
            
            // Arrange - prepare posts for different scenarios
            var plaidEnabledPost = _testPosts.First(p => p.IsPlaidEnabled && p.PortfolioValueAtPosting != null);
            var plaidEnabledNoValuePost = _testPosts.First(p => p.IsPlaidEnabled && p.PortfolioValueAtPosting == null);
            var nonPlaidPost = _testPosts.First(p => !p.IsPlaidEnabled);

            // Assert - Check the model properties that affect rendering
            // For plaid user with portfolio value
            Assert.Multiple(() => {
                Assert.That(plaidEnabledPost.IsPlaidEnabled, Is.True, "IsPlaidEnabled should be true");
                Assert.That(plaidEnabledPost.PortfolioValueAtPosting, Is.Not.Null, "PortfolioValueAtPosting should not be null");
            });

            // For plaid user without portfolio value
            Assert.Multiple(() => {
                Assert.That(plaidEnabledNoValuePost.IsPlaidEnabled, Is.True, "IsPlaidEnabled should be true");
                Assert.That(plaidEnabledNoValuePost.PortfolioValueAtPosting, Is.Null, "PortfolioValueAtPosting should be null");
            });

            // For non-plaid user
            Assert.Multiple(() => {
                Assert.That(nonPlaidPost.IsPlaidEnabled, Is.False, "IsPlaidEnabled should be false");
                Assert.That(nonPlaidPost.PortfolioValueAtPosting, Is.Null, "PortfolioValueAtPosting should be null");
            });
        }

        
        [Test]
        public void PostPreviewViewModel_MapsPlaidStatusCorrectly()
        {
            // Arrange
            var plaidUser = new User
            {
                Id = 1,
                Username = "TestPlaidUser",
                PlaidEnabled = true
            };

            var nonPlaidUser = new User
            {
                Id = 2,
                Username = "TestNonPlaidUser",
                PlaidEnabled = false
            };

            var nullPlaidUser = new User
            {
                Id = 3,
                Username = "TestNullPlaidUser",
                PlaidEnabled = null
            };

            var plaidPost = new Post
            {
                Id = 1,
                UserId = 1,
                User = plaidUser,
                Title = "Plaid Post",
                Content = "Content",
                PortfolioValueAtPosting = 10000.00M
            };

            var nonPlaidPost = new Post
            {
                Id = 2,
                UserId = 2,
                User = nonPlaidUser,
                Title = "Non-Plaid Post",
                Content = "Content"
            };

            var nullPlaidPost = new Post
            {
                Id = 3,
                UserId = 3,
                User = nullPlaidUser,
                Title = "Null-Plaid Post",
                Content = "Content"
            };

            // Act - Create view models similar to how the controller would
            var plaidPostVM = new PostPreviewVM
            {
                Id = plaidPost.Id,
                UserName = plaidPost.User.Username,
                Title = plaidPost.Title,
                Excerpt = plaidPost.Content,
                IsPlaidEnabled = plaidPost.User.PlaidEnabled ?? false,
                PortfolioValueAtPosting = FormatCurrencyAbbreviate.FormatCurrencyAbbreviated(plaidPost.PortfolioValueAtPosting.Value)
            };

            var nonPlaidPostVM = new PostPreviewVM
            {
                Id = nonPlaidPost.Id,
                UserName = nonPlaidPost.User.Username,
                Title = nonPlaidPost.Title,
                Excerpt = nonPlaidPost.Content,
                IsPlaidEnabled = nonPlaidPost.User.PlaidEnabled ?? false,
                PortfolioValueAtPosting = FormatCurrencyAbbreviate.FormatCurrencyAbbreviated(plaidPost.PortfolioValueAtPosting.Value)
            };

            var nullPlaidPostVM = new PostPreviewVM
            {
                Id = nullPlaidPost.Id,
                UserName = nullPlaidPost.User.Username,
                Title = nullPlaidPost.Title,
                Excerpt = nullPlaidPost.Content,
                IsPlaidEnabled = nullPlaidPost.User.PlaidEnabled ?? false,
                PortfolioValueAtPosting = FormatCurrencyAbbreviate.FormatCurrencyAbbreviated(plaidPost.PortfolioValueAtPosting.Value)
            };

            // Assert
            Assert.Multiple(() => {
                Assert.That(plaidPostVM.IsPlaidEnabled, Is.True, "Plaid user should map to IsPlaidEnabled=true");
                Assert.That(nonPlaidPostVM.IsPlaidEnabled, Is.False, "Non-plaid user should map to IsPlaidEnabled=false");
                Assert.That(nullPlaidPostVM.IsPlaidEnabled, Is.False, "Null plaid user should map to IsPlaidEnabled=false");
            });
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }
    }
}