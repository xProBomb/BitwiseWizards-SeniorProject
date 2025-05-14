using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using TrustTrade.Controllers;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using TrustTrade.Services;
using TrustTrade.Services.Web.Interfaces;

namespace TestTrustTrade
{
    [TestFixture]
    public class ProfileBackgroundImageTests
    {
        private TrustTradeDbContext _context;
        private Mock<ILogger<ProfileController>> _loggerMock;
        private Mock<IHoldingsRepository> _holdingsRepositoryMock;
        private Mock<IPostService> _postServiceMock;
        private Mock<IProfileService> _profileServiceMock;
        private Mock<IUserBlockRepository> _userBlockRepositoryMock;
        private Mock<IUserService> _userServiceMock;
        private Mock<IPerformanceScoreRepository> _performanceScoreRepositoryMock;
        private Mock<INotificationService> _notificationServiceMock;
        private ProfileController _controller;
        private User _testUser;

        [SetUp]
        public void Setup()
        {
            // Create an in-memory database
            var options = new DbContextOptionsBuilder<TrustTradeDbContext>()
                .UseInMemoryDatabase(databaseName: $"ProfileTest_{Guid.NewGuid()}")
                .Options;

            _context = new TrustTradeDbContext(options);
            
            // Create the test user
            _testUser = new User
            {
                Id = 1,
                IdentityId = "test-user-id",
                Username = "testuser",
                PasswordHash = "testpassword",
                ProfileName = "Test User",
                Email = "test@example.com"
            };
            
            // Add test user to the in-memory database
            _context.Users.Add(_testUser);
            _context.SaveChanges();

            // Create service mocks
            _loggerMock = new Mock<ILogger<ProfileController>>();
            _holdingsRepositoryMock = new Mock<IHoldingsRepository>();
            _postServiceMock = new Mock<IPostService>();
            _profileServiceMock = new Mock<IProfileService>();
            _userBlockRepositoryMock = new Mock<IUserBlockRepository>();
            _userServiceMock = new Mock<IUserService>();
            _performanceScoreRepositoryMock = new Mock<IPerformanceScoreRepository>();
            _notificationServiceMock = new Mock<INotificationService>();

            // Set up controller with real in-memory DbContext
            _controller = new ProfileController(
                _context,
                _holdingsRepositoryMock.Object,
                _loggerMock.Object,
                _postServiceMock.Object,
                _profileServiceMock.Object,
                _userBlockRepositoryMock.Object,
                _userServiceMock.Object,
                _performanceScoreRepositoryMock.Object,
                _notificationServiceMock.Object
            );

            // Set up controller context with the user identity
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
            
            // Set up TempData for the controller
            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(), 
                Mock.Of<ITempDataProvider>());
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
            _controller.Dispose();
        }

        [Test]
        public async Task UpdateBackgroundImage_WithUrl_SetsBackgroundImageUrl()
        {
            // Arrange
            const string imageUrl = "https://media1.tenor.com/m/2roX3uxz_68AAAAC/cat-space.gif";
            
            // Act
            var result = await _controller.UpdateBackgroundImage(null, imageUrl, "Url") as RedirectToActionResult;
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("MyProfile"));
            
            // Retrieve updated user to verify changes
            var updatedUser = await _context.Users.FirstOrDefaultAsync(u => u.IdentityId == "test-user-id");
            Assert.That(updatedUser, Is.Not.Null);
            Assert.That(updatedUser.BackgroundImageUrl, Is.EqualTo(imageUrl));
            Assert.That(updatedUser.BackgroundSource, Is.EqualTo("Url"));
            Assert.That(updatedUser.BackgroundImage, Is.Null);
        }

        [Test]
        public async Task UpdateBackgroundImage_WithInvalidUrl_AddsErrorToTempData()
        {
            // Arrange
            const string invalidUrl = "not-a-valid-url";
            
            // Act
            var result = await _controller.UpdateBackgroundImage(null, invalidUrl, "Url") as RedirectToActionResult;
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("MyProfile"));
            Assert.That(_controller.TempData["BackgroundImageError"], Is.Not.Null);
            
            // Verify user was not updated
            var updatedUser = await _context.Users.FirstOrDefaultAsync(u => u.IdentityId == "test-user-id");
            Assert.That(updatedUser.BackgroundImageUrl, Is.Null);
        }

        [Test]
        public async Task RemoveBackgroundImage_ClearsAllBackgroundImageProperties()
        {
            // Arrange - set initial values
            var user = await _context.Users.FirstOrDefaultAsync(u => u.IdentityId == "test-user-id");
            user.BackgroundImageUrl = "https://example.com/image.jpg";
            user.BackgroundSource = "Url";
            user.BackgroundImage = new byte[] { 1, 2, 3 };
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _controller.RemoveBackgroundImage() as RedirectToActionResult;
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("MyProfile"));
            
            // Verify properties were cleared
            var updatedUser = await _context.Users.FirstOrDefaultAsync(u => u.IdentityId == "test-user-id");
            Assert.That(updatedUser.BackgroundImageUrl, Is.Null);
            Assert.That(updatedUser.BackgroundSource, Is.Null);
            Assert.That(updatedUser.BackgroundImage, Is.Null);
        }

        [Test]
        public async Task UpdateBackgroundImage_WithTooLargeFile_AddsErrorToTempData()
        {
            // Arrange
            var formFile = new Mock<IFormFile>();
            formFile.Setup(f => f.Length).Returns(6 * 1024 * 1024); // 6MB - over the 5MB limit
            formFile.Setup(f => f.ContentType).Returns("image/jpeg");
            
            // Act
            var result = await _controller.UpdateBackgroundImage(formFile.Object, null, "File") as RedirectToActionResult;
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("MyProfile"));
            Assert.That(_controller.TempData["BackgroundImageError"], Is.Not.Null);
            
            // Verify user was not updated
            var updatedUser = await _context.Users.FirstOrDefaultAsync(u => u.IdentityId == "test-user-id");
            Assert.That(updatedUser.BackgroundImage, Is.Null);
        }

        [Test]
        public async Task UpdateBackgroundImage_WithInvalidFileType_AddsErrorToTempData()
        {
            // Arrange
            var formFile = new Mock<IFormFile>();
            formFile.Setup(f => f.Length).Returns(100 * 1024); // 100KB
            formFile.Setup(f => f.ContentType).Returns("text/plain"); // Not an image
            
            // Act
            var result = await _controller.UpdateBackgroundImage(formFile.Object, null, "File") as RedirectToActionResult;
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("MyProfile"));
            Assert.That(_controller.TempData["BackgroundImageError"], Is.Not.Null);
            
            // Verify user was not updated
            var updatedUser = await _context.Users.FirstOrDefaultAsync(u => u.IdentityId == "test-user-id");
            Assert.That(updatedUser.BackgroundImage, Is.Null);
        }
    }
}