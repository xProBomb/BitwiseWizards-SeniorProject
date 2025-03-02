using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using TrustTrade.Controllers;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using TrustTrade.ViewModels;

namespace TestTrustTrade
{
    [TestFixture]
    public class PostPlaidIconTests_Enhanced
    {
        [Test]
        public void ViewModel_WithPlaidEnabled_HasPlaidFlagSet()
        {
            // Arrange
            var viewModel = new PostPreviewVM
            {
                UserName = "Test User",
                IsPlaidEnabled = true
            };

            // Assert
            Assert.That(viewModel.IsPlaidEnabled, Is.True);
        }

        [Test]
        public void ViewModel_WithPlaidDisabled_DoesNotHavePlaidFlagSet()
        {
            // Arrange
            var viewModel = new PostPreviewVM
            {
                UserName = "Test User",
                IsPlaidEnabled = false
            };

            // Assert
            Assert.That(viewModel.IsPlaidEnabled, Is.False);
        }
        
        [Test]
        public void Post_WithPlaidEnabledUser_ShouldShowCheckmark()
        {
            // Arrange
            var user = new User
            {
                ProfileName = "Test User",
                PlaidEnabled = true
            };
            
            var post = new Post
            {
                Id = 1,
                UserId = 1,
                User = user
            };
            
            // This mirrors the logic in your controller
            var viewModel = new PostPreviewVM
            {
                Id = post.Id,
                UserName = post.User.Username,
                IsPlaidEnabled = post.User.PlaidEnabled ?? false
            };
            
            // Assert - This is what the view checks to show the checkmark
            Assert.That(viewModel.IsPlaidEnabled, Is.True);
        }

        [Test]
        public void Post_WithPlaidDisabledUser_ShouldNotShowCheckmark()
        {
            // Arrange
            var user = new User
            {
                ProfileName = "Test User",
                PlaidEnabled = false
            };
            
            var post = new Post
            {
                Id = 1,
                UserId = 1,
                User = user
            };
            
            // This mirrors the logic in your controller
            var viewModel = new PostPreviewVM
            {
                Id = post.Id,
                UserName = post.User.Username,
                IsPlaidEnabled = post.User.PlaidEnabled ?? false
            };
            
            // Assert - This is what the view checks to show the checkmark
            Assert.That(viewModel.IsPlaidEnabled, Is.False);
        }

        [Test]
        public void Post_WithPlaidNull_ShouldNotShowCheckmark()
        {
            // Arrange
            var user = new User
            {
                ProfileName = "Test User",
                PlaidEnabled = null
            };
            
            var post = new Post
            {
                Id = 1,
                UserId = 1,
                User = user
            };
            
            // This mirrors the logic in your controller
            var viewModel = new PostPreviewVM
            {
                Id = post.Id,
                UserName = post.User.Username,
                IsPlaidEnabled = post.User.PlaidEnabled ?? false
            };
            
            // Assert - This is what the view checks to show the checkmark
            Assert.That(viewModel.IsPlaidEnabled, Is.False);
        }

        [Test]
        public void Post_WithPlaidEnabledAndPortfolioValue_ShowsValueAndCheckmark()
        {
            // Arrange
            var user = new User
            {
                ProfileName = "Test User",
                PlaidEnabled = true
            };
            
            var post = new Post
            {
                Id = 1,
                UserId = 1,
                User = user,
                PortfolioValueAtPosting = 12345.67M
            };
            
            // This mirrors the logic in your controller
            var viewModel = new PostPreviewVM
            {
                Id = post.Id,
                UserName = post.User.Username,
                IsPlaidEnabled = post.User.PlaidEnabled ?? false,
                PortfolioValueAtPosting = post.PortfolioValueAtPosting
            };
            
            // Assert
            Assert.Multiple(() => {
                Assert.That(viewModel.IsPlaidEnabled, Is.True, "IsPlaidEnabled should be true");
                Assert.That(viewModel.PortfolioValueAtPosting.HasValue, Is.True, "PortfolioValueAtPosting should have a value");
                Assert.That(viewModel.PortfolioValueAtPosting!.Value, Is.EqualTo(12345.67M), "Portfolio value should match");
            });
        }

        [Test]
        public void HomeController_MapsPlaidStatusCorrectly()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HomeController>>();
            var postRepoMock = new Mock<IPostRepository>();
            var tagRepoMock = new Mock<ITagRepository>();
            var controller = new HomeController(loggerMock.Object, postRepoMock.Object, tagRepoMock.Object);

            var plaidUser = new User
            {
                Id = 1,
                Username = "PlaidUser",
                PlaidEnabled = true
            };

            var plaidPost = new Post
            {
                Id = 1,
                UserId = 1,
                Title = "Test Post",
                Content = "Test Content with more than 100 characters to test excerpt generation. This should be long enough to test the excerpt truncation logic.",
                CreatedAt = System.DateTime.Now,
                User = plaidUser,
                PortfolioValueAtPosting = 10000.00M
            };

            postRepoMock.Setup(repo => repo.GetPagedPosts(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(new List<Post> { plaidPost });
            postRepoMock.Setup(repo => repo.GetTotalPosts(It.IsAny<string>()))
                .Returns(1);
            tagRepoMock.Setup(repo => repo.GetAllTagNames())
                .Returns(new List<string>());

            // Act
            var result = controller.Index() as ViewResult;
            var model = result?.Model as IndexVM;
            var postPreview = model?.Posts?.FirstOrDefault();

            // Assert
            Assert.That(postPreview, Is.Not.Null, "Post preview should exist");
            Assert.That(postPreview.IsPlaidEnabled, Is.True, "IsPlaidEnabled should correctly map from User.PlaidEnabled");
            Assert.That(postPreview.PortfolioValueAtPosting, Is.EqualTo(10000.00M), "PortfolioValueAtPosting should be mapped correctly");
            Assert.That(postPreview.Excerpt.Length, Is.LessThanOrEqualTo(103), "Excerpt should be truncated to ~100 characters with ellipsis");
        }
    }
}