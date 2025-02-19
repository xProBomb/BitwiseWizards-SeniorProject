using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestIdentity
{
    [TestFixture]
    public class IdentityTests
    {
        private Mock<IUserPasswordStore<IdentityUser>> _mockUserStore;
        private Mock<IPasswordHasher<IdentityUser>> _mockPasswordHasher;
        private Mock<IPasswordValidator<IdentityUser>> _mockPasswordValidator;
        private UserManager<IdentityUser> _userManager;

        [SetUp]
        public void Setup()
        {
            // 1) Mock the IUserPasswordStore instead of just IUserStore
            _mockUserStore = new Mock<IUserPasswordStore<IdentityUser>>();

            // Minimal setups so UserManager can set/get the password hash
            _mockUserStore
                .Setup(s => s.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserStore
                .Setup(s => s.SetPasswordHashAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockUserStore
                .Setup(s => s.GetPasswordHashAsync(It.IsAny<IdentityUser>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("FakeHashedPassword");
            _mockUserStore
                .Setup(s => s.HasPasswordAsync(It.IsAny<IdentityUser>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // 2) Mock the password hasher
            _mockPasswordHasher = new Mock<IPasswordHasher<IdentityUser>>();
            // By default, let's not override VerifyHashedPassword (we'll do it in specific tests)

            // 3) Mock the password validator
            _mockPasswordValidator = new Mock<IPasswordValidator<IdentityUser>>();
            // By default, let the password validator succeed
            _mockPasswordValidator
                .Setup(v => v.ValidateAsync(
                    It.IsAny<UserManager<IdentityUser>>(),
                    It.IsAny<IdentityUser>(),
                    It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // 4) Create the UserManager
            _userManager = new UserManager<IdentityUser>(
                _mockUserStore.Object,
                Options.Create(new IdentityOptions()),
                _mockPasswordHasher.Object,
                new IUserValidator<IdentityUser>[0],  // or provide your own user validator mocks
                new[] { _mockPasswordValidator.Object },
                new Mock<ILookupNormalizer>().Object,
                new IdentityErrorDescriber(),
                null, // No IServiceProvider
                new Mock<ILogger<UserManager<IdentityUser>>>().Object
            );
        }

        [TearDown]
        public void Teardown()
        {
            _userManager?.Dispose();
        }

        [Test]
        public async Task CreateUserAsync_ValidPassword_ShouldSucceed()
        {
            // Arrange
            var user = new IdentityUser { UserName = "validUser", Email = "valid@example.com" };
            var password = "ValidPassword123!";

            // Act
            var result = await _userManager.CreateAsync(user, password);

            // Assert
            Assert.That(result.Succeeded, Is.True, "Expected user creation to succeed.");
            _mockUserStore.Verify(
                s => s.CreateAsync(user, It.IsAny<CancellationToken>()),
                Times.Once,
                "CreateAsync should be called exactly once."
            );
            _mockUserStore.Verify(
                s => s.SetPasswordHashAsync(user, It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once,
                "SetPasswordHashAsync should be called to store the hashed password."
            );
        }

        [Test]
        public async Task CreateUserAsync_InvalidPassword_ShouldFail()
        {
            // Arrange
            var user = new IdentityUser { UserName = "invalidUser", Email = "invalid@example.com" };
            var invalidPassword = "short";

            // Force the password validator to return an error
            _mockPasswordValidator
                .Setup(v => v.ValidateAsync(
                    It.IsAny<UserManager<IdentityUser>>(),
                    user,
                    invalidPassword))
                .ReturnsAsync(IdentityResult.Failed(
                    new IdentityError { Description = "Password too short." }));

            // Act
            var result = await _userManager.CreateAsync(user, invalidPassword);

            // Assert
            Assert.That(result.Succeeded, Is.False, "Expected user creation to fail with an invalid password.");
            Assert.That(result.Errors, Has.One.Items, "Should have exactly one error.");
            Assert.That(
                result.Errors,
                Has.Some.Matches<IdentityError>(e => e.Description.Contains("too short")),
                "Expected error message about short password."
            );
        }

        [Test]
        public async Task CheckPasswordAsync_CorrectPassword_ShouldReturnTrue()
        {
            // Arrange
            var user = new IdentityUser { UserName = "testuser" };
            var correctPassword = "CorrectPassword123!";

            // Mock the password hasher to succeed if the provided password matches
            _mockPasswordHasher
                .Setup(h => h.VerifyHashedPassword(user, "FakeHashedPassword", correctPassword))
                .Returns(PasswordVerificationResult.Success);

            // Act
            var result = await _userManager.CheckPasswordAsync(user, correctPassword);

            // Assert
            Assert.That(result, Is.True, "CheckPasswordAsync should return true for the correct password.");
        }

        [Test]
        public async Task CheckPasswordAsync_IncorrectPassword_ShouldReturnFalse()
        {
            // Arrange
            var user = new IdentityUser { UserName = "testuser" };
            var incorrectPassword = "WrongPassword";

            // Mock the password hasher to fail if the provided password doesn't match
            _mockPasswordHasher
                .Setup(h => h.VerifyHashedPassword(user, "FakeHashedPassword", incorrectPassword))
                .Returns(PasswordVerificationResult.Failed);

            // Act
            var result = await _userManager.CheckPasswordAsync(user, incorrectPassword);

            // Assert
            Assert.That(result, Is.False, "CheckPasswordAsync should return false for an incorrect password.");
        }
    }

    
}
