using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Logging;

using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrustTrade.Controllers;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using TrustTrade.ViewModels;

namespace TestTrustTrade
{
    [TestFixture]
    public class HomeIndexViewTests
    {
        private Mock<ILogger<HomeController>> _loggerMock;
        private Mock<IPostRepository> _postRepositoryMock;
        private Mock<ITagRepository> _tagRepositoryMock;
        private HomeController _controller;
        private List<Post> _mockPosts;
        private List<User> _mockUsers;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<HomeController>>();
            _postRepositoryMock = new Mock<IPostRepository>();
            _tagRepositoryMock = new Mock<ITagRepository>();
            _controller = new HomeController(_loggerMock.Object, _postRepositoryMock.Object, _tagRepositoryMock.Object);

            // Setup mock users
            _mockUsers = new List<User>
            {
                new User 
                { 
                    Id = 1, 
                    Username = "PlaidUser", 
                    ProfileName = "Plaid User", 
                    Email = "plaid@example.com", 
                    PasswordHash = "hash123", 
                    PlaidEnabled = true 
                },
                new User 
                { 
                    Id = 2, 
                    Username = "NonPlaidUser", 
                    ProfileName = "Non-Plaid User", 
                    Email = "nonplaid@example.com",
                    PasswordHash = "hash456", 
                    PlaidEnabled = false 
                },
                new User 
                { 
                    Id = 3, 
                    Username = "NullPlaidUser", 
                    ProfileName = "Null Plaid User", 
                    Email = "nullplaid@example.com",
                    PasswordHash = "hash789", 
                    PlaidEnabled = null 
                }
            };

            // Setup mock posts
            _mockPosts = new List<Post>
            {
                new Post 
                { 
                    Id = 1,
                    UserId = 1, 
                    Title = "Post with Plaid",
                    Content = "This post is from a user with Plaid enabled",
                    CreatedAt = System.DateTime.Now,
                    PortfolioValueAtPosting = 12345.67M,
                    User = _mockUsers[0]
                },
                new Post 
                { 
                    Id = 2,
                    UserId = 2, 
                    Title = "Post without Plaid",
                    Content = "This post is from a user without Plaid enabled",
                    CreatedAt = System.DateTime.Now.AddHours(-1),
                    User = _mockUsers[1] 
                },
                new Post 
                { 
                    Id = 3,
                    UserId = 3, 
                    Title = "Post with null Plaid status",
                    Content = "This post is from a user with null Plaid status",
                    CreatedAt = System.DateTime.Now.AddHours(-2),
                    User = _mockUsers[2]
                },
                new Post 
                { 
                    Id = 4,
                    UserId = 1, 
                    Title = "Another Plaid post, no portfolio value",
                    Content = "This post is from a Plaid user but has no portfolio value",
                    CreatedAt = System.DateTime.Now.AddHours(-3),
                    PortfolioValueAtPosting = null,
                    User = _mockUsers[0]
                }
            };
        }

        [Test]
        public void Index_ReturnsAViewResult()
        {
            // Arrange
            _postRepositoryMock.Setup(repo => repo.GetPagedPosts(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(_mockPosts);
            _postRepositoryMock.Setup(repo => repo.GetTotalPosts(It.IsAny<string>()))
                .Returns(_mockPosts.Count);
            _tagRepositoryMock.Setup(repo => repo.GetAllTagNames())
                .Returns(new List<string>());

            // Act
            var result = _controller.Index();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public void Index_ReturnsCorrectModelType()
        {
            // Arrange
            _postRepositoryMock.Setup(repo => repo.GetPagedPosts(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(_mockPosts);
            _postRepositoryMock.Setup(repo => repo.GetTotalPosts(It.IsAny<string>()))
                .Returns(_mockPosts.Count);
            _tagRepositoryMock.Setup(repo => repo.GetAllTagNames())
                .Returns(new List<string>());

            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.That(result?.Model, Is.Not.Null);
            Assert.That(result?.Model, Is.InstanceOf<IndexVM>());
        }

        [Test]
        public void Index_PlaidEnabledUser_ShowsCheckmarkInViewModel()
        {
            // Arrange
            _postRepositoryMock.Setup(repo => repo.GetPagedPosts(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(_mockPosts);
            _postRepositoryMock.Setup(repo => repo.GetTotalPosts(It.IsAny<string>()))
                .Returns(_mockPosts.Count);
            _tagRepositoryMock.Setup(repo => repo.GetAllTagNames())
                .Returns(new List<string>());

            // Act
            var result = _controller.Index() as ViewResult;
            var model = result?.Model as IndexVM;
            
            // Find posts by users with different Plaid statuses
            var plaidEnabledPost = model?.Posts?.FirstOrDefault(p => p.Id == 1);
            var nonPlaidPost = model?.Posts?.FirstOrDefault(p => p.Id == 2);
            var nullPlaidPost = model?.Posts?.FirstOrDefault(p => p.Id == 3);

            // Assert
            Assert.That(model, Is.Not.Null, "Model should not be null");
            Assert.That(model.Posts, Is.Not.Null, "Posts collection should not be null");
            
            Assert.That(plaidEnabledPost, Is.Not.Null, "Plaid-enabled post should exist in model");
            Assert.That(plaidEnabledPost.IsPlaidEnabled, Is.True, "IsPlaidEnabled should be true for Plaid user");
            
            Assert.That(nonPlaidPost, Is.Not.Null, "Non-Plaid post should exist in model");
            Assert.That(nonPlaidPost.IsPlaidEnabled, Is.False, "IsPlaidEnabled should be false for non-Plaid user");
            
            Assert.That(nullPlaidPost, Is.Not.Null, "Null-Plaid post should exist in model");
            Assert.That(nullPlaidPost.IsPlaidEnabled, Is.False, "IsPlaidEnabled should be false for null Plaid status");
        }

        [Test]
        public void Index_PlaidEnabledUserWithPortfolioValue_IncludesValueInViewModel()
        {
            // Arrange
            _postRepositoryMock.Setup(repo => repo.GetPagedPosts(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(_mockPosts);
            _postRepositoryMock.Setup(repo => repo.GetTotalPosts(It.IsAny<string>()))
                .Returns(_mockPosts.Count);
            _tagRepositoryMock.Setup(repo => repo.GetAllTagNames())
                .Returns(new List<string>());

            // Act
            var result = _controller.Index() as ViewResult;
            var model = result?.Model as IndexVM;
            
            // Find posts with different portfolio values
            var postWithValue = model?.Posts?.FirstOrDefault(p => p.Id == 1);
            var postWithoutValue = model?.Posts?.FirstOrDefault(p => p.Id == 4);

            // Assert
            Assert.That(model, Is.Not.Null, "Model should not be null");
            Assert.That(model.Posts, Is.Not.Null, "Posts collection should not be null");
            
            Assert.That(postWithValue, Is.Not.Null, "Post with portfolio value should exist in model");
            Assert.That(postWithValue.IsPlaidEnabled, Is.True, "IsPlaidEnabled should be true");
            Assert.That(postWithValue.PortfolioValueAtPosting, Is.EqualTo(12345.67M), "Portfolio value should match expected");
            
            Assert.That(postWithoutValue, Is.Not.Null, "Post without portfolio value should exist in model");
            Assert.That(postWithoutValue.IsPlaidEnabled, Is.True, "IsPlaidEnabled should be true even without portfolio value");
            Assert.That(postWithoutValue.PortfolioValueAtPosting, Is.Null, "Portfolio value should be null when not provided");
        }

        [Test]
        public void Index_PagingAndSorting_PreservesPlaidStatusInViewModel()
        {
            // Arrange
            var sortedPosts = _mockPosts.OrderBy(p => p.CreatedAt).ToList(); // Sort by date ascending
            _postRepositoryMock.Setup(repo => repo.GetPagedPosts(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), "DateAsc"))
                .Returns(sortedPosts);
            _postRepositoryMock.Setup(repo => repo.GetTotalPosts(It.IsAny<string>()))
                .Returns(_mockPosts.Count);
            _tagRepositoryMock.Setup(repo => repo.GetAllTagNames())
                .Returns(new List<string>());

            // Act
            var result = _controller.Index(sortOrder: "DateAsc") as ViewResult;
            var model = result?.Model as IndexVM;

            // Assert
            Assert.That(model, Is.Not.Null, "Model should not be null");
            Assert.That(model.Posts, Is.Not.Null, "Posts collection should not be null");
            Assert.That(model.SortOrder, Is.EqualTo("DateAsc"), "Sort order should be preserved");
            
            // Verify that the plaid status is correctly mapped regardless of sorting
            var plaidEnabledPosts = model.Posts.Where(p => p.IsPlaidEnabled).ToList();
            var nonPlaidPosts = model.Posts.Where(p => !p.IsPlaidEnabled).ToList();
            
            Assert.That(plaidEnabledPosts.Count, Is.EqualTo(2), "Should have 2 posts with Plaid enabled");
            Assert.That(nonPlaidPosts.Count, Is.EqualTo(2), "Should have 2 posts without Plaid enabled");
        }

        [Test]
        public void ConvertToViewModel_CorrectlyMapsPlaidStatus()
        {
            // This test validates the mapping logic in the controller
            // We'll create mock data similar to what the repository would return

            // Arrange
            var mockPostsWithUsers = new List<Post> {
                new Post { 
                    Id = 5, 
                    Title = "Test Post", 
                    Content = "Test Content", 
                    CreatedAt = System.DateTime.Now,
                    PortfolioValueAtPosting = 9999.99M,
                    User = new User { 
                        Username = "TestUser", 
                        PlaidEnabled = true 
                    }
                }
            };

            _postRepositoryMock.Setup(repo => repo.GetPagedPosts(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(mockPostsWithUsers);
            _postRepositoryMock.Setup(repo => repo.GetTotalPosts(It.IsAny<string>()))
                .Returns(1);
            _tagRepositoryMock.Setup(repo => repo.GetAllTagNames())
                .Returns(new List<string>());

            // Act
            var result = _controller.Index() as ViewResult;
            var model = result?.Model as IndexVM;
            var postPreview = model?.Posts?.FirstOrDefault();

            // Assert
            Assert.That(model, Is.Not.Null, "Model should not be null");
            Assert.That(postPreview, Is.Not.Null, "Post preview should not be null");
            Assert.That(postPreview.IsPlaidEnabled, Is.True, "IsPlaidEnabled should be true");
            Assert.That(postPreview.PortfolioValueAtPosting, Is.EqualTo(9999.99M), "Portfolio value should be mapped correctly");
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }
    }
}