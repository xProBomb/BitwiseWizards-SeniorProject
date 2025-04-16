using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using TrustTrade.Controllers;
using TrustTrade.Data;
using TrustTrade.Models;
using TrustTrade.Services.Web.Interfaces;
using TrustTrade.ViewModels;

namespace TrustTrade.Tests.Controllers
{
    [TestFixture]
    public class SearchControllerTests
    {
        private Mock<ISearchUserRepository> _userRepoMock;
        private Mock<IPostService> _postServiceMock;
        private SearchController _controller;
        private List<PostPreviewVM> _postPreviews;
        private PostFiltersPartialVM _postFiltersPartialVM;
        private PaginationPartialVM _paginationPartialVM;

        [SetUp]
        public void SetUp()
        {
            _userRepoMock = new Mock<ISearchUserRepository>();
            _postServiceMock = new Mock<IPostService>();
            _controller = new SearchController(
                _userRepoMock.Object, 
                _postServiceMock.Object);

            _postPreviews = new List<PostPreviewVM>
            {
                new PostPreviewVM
                    {
                        Id = 1,
                        Title = "Post 1",
                        IsPlaidEnabled = true
                    }
            };

            _postFiltersPartialVM = new PostFiltersPartialVM
            {
                SelectedCategory = "Category1",
            };

            _paginationPartialVM = new PaginationPartialVM
            {
                CurrentPage = 1,
                TotalPages = 3
            };
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

        [Test]
        public async Task SearchPosts_ReturnsAViewResult()
        {
            // Arrange
            _postServiceMock.Setup(s => s.SearchPostsAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(_postPreviews);
            _postServiceMock.Setup(s => s.BuildPostFiltersAsync(It.IsAny<string>(), It.IsAny<string>(), null))
                .ReturnsAsync(_postFiltersPartialVM);
            _postServiceMock.Setup(s => s.BuildSearchPaginationAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(_paginationPartialVM);

            // Act
            var result = await _controller.SearchPosts("test search");

            // Assert
            Assert.That(result, Is.Not.Null.And.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task SearchPosts_ReturnsSearchPostResultsVM()
        {
            // Arrange
            _postServiceMock.Setup(s => s.SearchPostsAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(_postPreviews);
            _postServiceMock.Setup(s => s.BuildPostFiltersAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(_postFiltersPartialVM);
            _postServiceMock.Setup(s => s.BuildSearchPaginationAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(_paginationPartialVM);

            // Act
            var result = await _controller.SearchPosts("test search") as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.Not.Null);
            Assert.That(result.Model, Is.InstanceOf<SearchPostResultsVM>());
            Assert.That(((SearchPostResultsVM)result.Model).Posts, Is.EqualTo(_postPreviews));
            Assert.That(((SearchPostResultsVM)result.Model).PostFilters, Is.EqualTo(_postFiltersPartialVM));
            Assert.That(((SearchPostResultsVM)result.Model).Pagination, Is.EqualTo(_paginationPartialVM));
        }

        [Test]
        public async Task SearchPosts_EmptySearchTerm_RedirectsToSearchPage()
        {
            // Arrange
            string searchTerm = string.Empty;
            var emptyUsers = new List<User>();

            _userRepoMock.Setup(repo => repo.SearchUsersAsync(searchTerm))
                .ReturnsAsync(emptyUsers);

            // Act
            var result = await _controller.SearchPosts(searchTerm) as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Search"));
        }
    }
}
