using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using TrustTrade.Controllers;
using TrustTrade.Data;
using TrustTrade.Models;

namespace TrustTrade.Tests.Controllers
{
    [TestFixture]
    public class SearchControllerTests
    {
        private Mock<ISearchUserRepository> _userRepoMock;
        private SearchController _controller;

        [SetUp]
        public void SetUp()
        {
            _userRepoMock = new Mock<ISearchUserRepository>();
            _controller = new SearchController(_userRepoMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            if (_controller is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        [Test]
        public async Task SearchUsers_InvalidSearchTermLength_ReturnsBadRequest()
        {
            // Arrange: search term longer than 50 characters
            string longSearchTerm = new string('a', 51);

            // Act
            var result = await _controller.SearchUsers(longSearchTerm);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task SearchUsers_ValidSearchTerm_ReturnsPartialViewWithModel()
        {
            // Arrange
            string searchTerm = "john";
            var users = new List<User>
            {
                new User 
                { 
                    Id = 1, 
                    Username = "johnDoe", 
                    ProfileName = "johnDoe", 
                    Email = "johnDoe@example.com", 
                    PasswordHash = "dummyHash", 
                    CreatedAt = DateTime.Now 
                },
                new User 
                { 
                    Id = 2, 
                    Username = "johnSmith", 
                    ProfileName = "johnSmith", 
                    Email = "johnSmith@example.com", 
                    PasswordHash = "dummyHash", 
                    CreatedAt = DateTime.Now 
                }
            };

            _userRepoMock.Setup(repo => repo.SearchUsersAsync(searchTerm))
                .ReturnsAsync(users);

            // Act
            var result = await _controller.SearchUsers(searchTerm);

            // Assert
            var partialViewResult = result as PartialViewResult;
            Assert.That(partialViewResult, Is.Not.Null, "Expected a PartialViewResult.");
            Assert.That(partialViewResult.ViewName, Is.EqualTo("_UserSearchResultsPartial"));
            Assert.That(partialViewResult.Model, Is.EqualTo(users));
        }

        [Test]
        public async Task SearchUsers_NullSearchTerm_ReturnsPartialViewWithEmptyModel()
        {
            // Arrange
            string searchTerm = null;
            var emptyUsers = new List<User>();

            _userRepoMock.Setup(repo => repo.SearchUsersAsync(searchTerm))
                .ReturnsAsync(emptyUsers);

            // Act
            var result = await _controller.SearchUsers(searchTerm);

            // Assert
            var partialViewResult = result as PartialViewResult;
            Assert.That(partialViewResult, Is.Not.Null);
            Assert.That(partialViewResult.ViewName, Is.EqualTo("_UserSearchResultsPartial"));
            Assert.That(partialViewResult.Model, Is.EqualTo(emptyUsers));
        }

        [Test]
        public async Task SearchUsers_WhiteSpaceSearchTerm_ReturnsPartialViewWithEmptyModel()
        {
            // Arrange
            string searchTerm = "   ";
            var emptyUsers = new List<User>();

            _userRepoMock.Setup(repo => repo.SearchUsersAsync(searchTerm))
                .ReturnsAsync(emptyUsers);

            // Act
            var result = await _controller.SearchUsers(searchTerm);

            // Assert
            var partialViewResult = result as PartialViewResult;
            Assert.That(partialViewResult, Is.Not.Null);
            Assert.That(partialViewResult.ViewName, Is.EqualTo("_UserSearchResultsPartial"));
            Assert.That(partialViewResult.Model, Is.EqualTo(emptyUsers));
        }

        [Test]
        public async Task SearchUsers_NoMatchingUsers_ReturnsPartialViewWithEmptyModel()
        {
            // Arrange
            string searchTerm = "nonexistent";
            var emptyUsers = new List<User>();

            _userRepoMock.Setup(repo => repo.SearchUsersAsync(searchTerm))
                .ReturnsAsync(emptyUsers);

            // Act
            var result = await _controller.SearchUsers(searchTerm);

            // Assert
            var partialViewResult = result as PartialViewResult;
            Assert.That(partialViewResult, Is.Not.Null);
            Assert.That(partialViewResult.ViewName, Is.EqualTo("_UserSearchResultsPartial"));
            Assert.That(partialViewResult.Model, Is.EqualTo(emptyUsers));
        }
    }
}
