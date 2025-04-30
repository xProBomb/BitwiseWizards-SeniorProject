using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using TrustTrade.Controllers;
using TrustTrade.Models;
using TrustTrade.Services.Web.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using TrustTrade.DAL.Abstract;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TrustTrade.ViewModels;
using TrustTrade.Services.Web.Interfaces;
using TrustTrade.DAL.Abstract;


namespace TrustTrade.Tests.Controllers
{
    public class AdminControllerTests
    {
        private Mock<IAdminService> _adminServiceMock;
        private Mock<IPostRepository> _postRepoMock;
        private Mock<IUserService> _userServiceMock;
        private Mock<IEmailSender> _emailSenderMock;
        private Mock<ILogger<PostsController>> _loggerMock;
        private AdminController _controller;
        private ClaimsPrincipal _adminPrincipal;

        [SetUp]
        public void SetUp()
        {
            _adminServiceMock = new Mock<IAdminService>();
            _postRepoMock = new Mock<IPostRepository>();
            _userServiceMock = new Mock<IUserService>();
            _emailSenderMock = new Mock<IEmailSender>();
            _loggerMock = new Mock<ILogger<PostsController>>();

            _controller = new AdminController(
                _loggerMock.Object,
                _postRepoMock.Object,
                _userServiceMock.Object,
                _adminServiceMock.Object,
                _emailSenderMock.Object
            );

            _adminPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "admin-id")
            }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _adminPrincipal }
            };
        }

        [Test]
        public async Task ManageUsers_ReturnsViewWithUsers_WhenAdmin()
        {
            // Arrange
            var testUsers = new List<User> { new User { Id = 1, Username = "testuser" } };
            _adminServiceMock.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new User { IsAdmin = true });
            _adminServiceMock.Setup(s => s.GetAllTrustTradeUsersAsync())
                .ReturnsAsync(testUsers);

            // Act
            var result = await _controller.ManageUsers();

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            var model = viewResult.Model as List<User>;
            Assert.That(model.Count, Is.EqualTo(1));
            Assert.That(model[0].Username, Is.EqualTo("testuser"));
        }

        [Test]
        public async Task SuspendUser_ReturnsOk_WhenSuccessful()
        {
            var userId = 1;
            var user = new User { Id = userId, Username = "testuser", Email = "test@example.com" };

            _adminServiceMock.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new User { IsAdmin = true });
            _adminServiceMock.Setup(s => s.FindUserByIdAsync(userId)).ReturnsAsync(user);

            var dto = new AdminController.UserActionDto { userId = userId };

            var result = await _controller.SuspendUser(dto);

            Assert.That(result, Is.TypeOf<OkResult>());
            _adminServiceMock.Verify(s => s.SuspendUserAsync(userId), Times.Once);
        }

        [Test]
        public async Task UnsuspendUser_ReturnsOk_WhenSuccessful()
        {
            var userId = 2;
            var user = new User { Id = userId, Username = "SuspendedUser", PastUsername = "realname", Email = "reactivate@example.com" };

            _adminServiceMock.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new User { IsAdmin = true });
            _adminServiceMock.Setup(s => s.FindUserByIdAsync(userId)).ReturnsAsync(user);

            var dto = new AdminController.UserActionDto { userId = userId };

            var result = await _controller.UnsuspendUser(dto);

            Assert.That(result, Is.TypeOf<OkResult>());
            _adminServiceMock.Verify(s => s.UnsuspendUserAsync(userId), Times.Once);
        }

        [Test]
        public async Task SearchUsers_ReturnsPartialView_WhenAdmin()
        {
            _adminServiceMock.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new User { IsAdmin = true });

            _adminServiceMock.Setup(s => s.SearchTrustTradeUsersAsync("susp"))
                .ReturnsAsync(new List<User> { new User { Username = "SuspendedUser" } });

            var result = await _controller.SearchUsers("susp");

            var partial = result as PartialViewResult;
            Assert.That(partial, Is.Not.Null);
            Assert.That(partial.ViewName, Is.EqualTo("_UserListPartial"));
        }

        [TearDown]
        public void TearDown()
        {
            if (_controller is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

    }
}
