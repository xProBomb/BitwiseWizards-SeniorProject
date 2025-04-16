using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TrustTrade.Controllers;
using TrustTrade.Services.Web.Interfaces;
using TrustTrade.ViewModels;
using TrustTrade.Models;
using System.Security.Claims;

namespace TestTrustTrade
{
    [TestFixture]
    public class HomeIndexViewTests
    {
        private Mock<ILogger<HomeController>> _loggerMock;
        private Mock<IPostService> _postServiceMock;
        private Mock<IUserService> _userServiceMock;
        private HomeController _controller;
        private User _user;
        private List<PostPreviewVM> _postPreviews;
        private PostFiltersPartialVM _postFiltersPartialVM;
        private PaginationPartialVM _paginationPartialVM;

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

            _user = new User
            {
                Id = 1,
                IdentityId = "test-identity-1",
                ProfileName = "johnDoe",
                Username = "johnDoe",
                Email = "johndoe@example.com",
                PasswordHash = "dummyHash"
            };

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

        [Test]
        public async Task Index_ReturnsAViewResult()
        {
            // Arrange
            _postServiceMock.Setup(s => s.GetPostPreviewsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(_postPreviews);
            _postServiceMock.Setup(s => s.BuildPostFiltersAsync(It.IsAny<string>(), It.IsAny<string>(), null))
                .ReturnsAsync(_postFiltersPartialVM);
            _postServiceMock.Setup(s => s.BuildPaginationAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(_paginationPartialVM);

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.That(result, Is.Not.Null.And.InstanceOf<ViewResult>());
        }

        
        [Test]
        public async Task Index_ReturnsCorrectViewModel()
        {
            // Arrange
            _postServiceMock.Setup(s => s.GetPostPreviewsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(_postPreviews);
            _postServiceMock.Setup(s => s.BuildPostFiltersAsync(It.IsAny<string>(), It.IsAny<string>(), null))
                .ReturnsAsync(_postFiltersPartialVM);
            _postServiceMock.Setup(s => s.BuildPaginationAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(_paginationPartialVM);

            // Act
            var result = await _controller.Index() as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.Not.Null);
            Assert.That(result.Model, Is.InstanceOf<IndexVM>());
            Assert.That(((IndexVM)result.Model).Posts, Is.EqualTo(_postPreviews));
            Assert.That(((IndexVM)result.Model).PostFilters, Is.EqualTo(_postFiltersPartialVM));
            Assert.That(((IndexVM)result.Model).Pagination, Is.EqualTo(_paginationPartialVM));
        }

        [Test]
        public async Task Following_ReturnsAViewResult()
        {
            // Arrange
            _userServiceMock.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<bool>()))
                .ReturnsAsync(_user);
            _postServiceMock.Setup(s => s.GetFollowingPostPreviewsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(_postPreviews);
            _postServiceMock.Setup(s => s.BuildPostFiltersAsync(It.IsAny<string>(), It.IsAny<string>(), null))
                .ReturnsAsync(_postFiltersPartialVM);
            _postServiceMock.Setup(s => s.BuildFollowingPaginationAsync(1, It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(_paginationPartialVM);

            // Act
            var result = await _controller.Following();

            // Assert
            Assert.That(result, Is.Not.Null.And.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task Following_ReturnsCorrectViewModel()
        {
            // Arrange
            _userServiceMock.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<bool>()))
                .ReturnsAsync(_user);
            _postServiceMock.Setup(s => s.GetFollowingPostPreviewsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(_postPreviews);
            _postServiceMock.Setup(s => s.BuildPostFiltersAsync(It.IsAny<string>(), It.IsAny<string>(), null))
                .ReturnsAsync(_postFiltersPartialVM);
            _postServiceMock.Setup(s => s.BuildFollowingPaginationAsync(1, It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(_paginationPartialVM);

            // Act
            var result = await _controller.Following() as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.Not.Null);
            Assert.That(result.Model, Is.InstanceOf<IndexVM>());
            Assert.That(((IndexVM)result.Model).Posts, Is.EqualTo(_postPreviews));
            Assert.That(((IndexVM)result.Model).PostFilters, Is.EqualTo(_postFiltersPartialVM));
            Assert.That(((IndexVM)result.Model).Pagination, Is.EqualTo(_paginationPartialVM));
        }

        [Test]
        public async Task Following_WhenUserNotFound_ReturnsUnauthorized()
        {
            // Arrange
            _userServiceMock.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<bool>()))
                .ReturnsAsync(() => null);

            // Act
            var result = await _controller.Following();

            // Assert
            Assert.That(result, Is.Not.Null.And.InstanceOf<UnauthorizedResult>());
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }
    }
}
