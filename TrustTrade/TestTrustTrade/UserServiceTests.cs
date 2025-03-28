using Moq;
using TrustTrade.DAL.Abstract;
using TrustTrade.Services.Web.Implementations;
using TrustTrade.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace TestTrustTrade;

[TestFixture]
public class UserServiceTests
{
    private Mock<ILogger<UserService>> _loggerMock;
    private Mock<UserManager<IdentityUser>> _userManagerMock;
    private Mock<IUserRepository> _userRepositoryMock;
    private UserService _userService;

    private User _user1;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<UserService>>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!
        );
        _userRepositoryMock = new Mock<IUserRepository>();

        _userService = new UserService(
            _loggerMock.Object,
            _userManagerMock.Object,
            _userRepositoryMock.Object
        );

        // Test users
        _user1 = new User
        {
            Id = 1,
            IdentityId = "test-identity-1",
            ProfileName = "johnDoe",
            Username = "johnDoe",
            Email = "johndoe@example.com",
            PasswordHash = "dummyHash"
        };
    }

    [Test]
    public async Task GetCurrentUserAsync_WhenUserFound_ReturnsUser()
    {
        // Arrange
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(() => "test-identity-1");
        _userRepositoryMock.Setup(r => r.FindByIdentityIdAsync("test-identity-1"))
            .ReturnsAsync(_user1);
        
        // Act
        var result = await _userService.GetCurrentUserAsync(new ClaimsPrincipal());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(_user1.Id));
        Assert.That(result.IdentityId, Is.EqualTo(_user1.IdentityId));
        _userManagerMock.Verify(u => u.GetUserId(It.IsAny<ClaimsPrincipal>()), Times.Once);
        _userRepositoryMock.Verify(r => r.FindByIdentityIdAsync("test-identity-1"), Times.Once);
    }

    [Test]
    public async Task GetCurrentUserAsync_WhenIdentityUserNotFound_ReturnsNull()
    {
        // Arrange
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(() => null);

        // Act
        var result = await _userService.GetCurrentUserAsync(new ClaimsPrincipal());

        // Assert
        Assert.That(result, Is.Null);
        _userManagerMock.Verify(u => u.GetUserId(It.IsAny<ClaimsPrincipal>()), Times.Once);
    }

    [Test]
    public async Task GetCurrentUserAsync_WhenUserNotFound_ReturnsNull()
    {
        // Arrange
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(() => "test-identity-1");
        _userRepositoryMock.Setup(r => r.FindByIdentityIdAsync("test-identity-1"))
            .ReturnsAsync(() => null);

        // Act
        var result = await _userService.GetCurrentUserAsync(new ClaimsPrincipal());

        // Assert
        Assert.That(result, Is.Null);
        _userManagerMock.Verify(u => u.GetUserId(It.IsAny<ClaimsPrincipal>()), Times.Once);
        _userRepositoryMock.Verify(r => r.FindByIdentityIdAsync("test-identity-1"), Times.Once);
    }
}