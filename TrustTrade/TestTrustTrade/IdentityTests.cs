using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
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
        private Mock<IUserValidator<IdentityUser>> _mockUserValidator;
        private UserManager<IdentityUser> _userManager;

        [SetUp]
        public void Setup()
        {
            // 1) Mock the IUserPasswordStore (which implements IUserStore)
            _mockUserStore = new Mock<IUserPasswordStore<IdentityUser>>();
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
            

            // 3) Mock the password validator to succeed by default
            _mockPasswordValidator = new Mock<IPasswordValidator<IdentityUser>>();
            _mockPasswordValidator
                .Setup(v => v.ValidateAsync(
                    It.IsAny<UserManager<IdentityUser>>(),
                    It.IsAny<IdentityUser>(),
                    It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // 4) Mock the user validator (for validating email, username, etc.)
            _mockUserValidator = new Mock<IUserValidator<IdentityUser>>();
            // By default, let the user validator succeed.
            _mockUserValidator
                .Setup(v => v.ValidateAsync(
                    It.IsAny<UserManager<IdentityUser>>(),
                    It.IsAny<IdentityUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // 5) Create the UserManager with our mocks.
            _userManager = new UserManager<IdentityUser>(
                _mockUserStore.Object,
                Options.Create(new IdentityOptions()),
                _mockPasswordHasher.Object,
                new IUserValidator<IdentityUser>[] { _mockUserValidator.Object },
                new[] { _mockPasswordValidator.Object },
                new Mock<ILookupNormalizer>().Object,
                new IdentityErrorDescriber(),
                null, 
                new Mock<ILogger<UserManager<IdentityUser>>>().Object
            );
        }

        [TearDown]
        public void Teardown()
        {
            _userManager?.Dispose();
        }

        #region Password Tests

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

            // Force the password validator to return an error.
            _mockPasswordValidator
                .Setup(v => v.ValidateAsync(_userManager, user, invalidPassword))
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

            // Mock the password hasher to succeed if the provided password matches.
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

            // Mock the password hasher to fail if the provided password doesn't match.
            _mockPasswordHasher
                .Setup(h => h.VerifyHashedPassword(user, "FakeHashedPassword", incorrectPassword))
                .Returns(PasswordVerificationResult.Failed);

            // Act
            var result = await _userManager.CheckPasswordAsync(user, incorrectPassword);

            // Assert
            Assert.That(result, Is.False, "CheckPasswordAsync should return false for an incorrect password.");
        }

        #endregion

        #region Email Validation Tests

        [Test]
        public async Task CreateUserAsync_ValidEmail_ShouldSucceed()
        {
            // Arrange
            var user = new IdentityUser { UserName = "validEmailUser", Email = "user@example.com" };
            var password = "ValidPassword123!";

            // Ensure the user validator returns success for a valid email.
            _mockUserValidator
                .Setup(v => v.ValidateAsync(_userManager, user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userManager.CreateAsync(user, password);

            // Assert
            Assert.That(result.Succeeded, Is.True, "Expected user creation to succeed with a valid email.");
        }

        [Test]
        public async Task CreateUserAsync_InvalidEmail_ShouldFail()
        {
            // Arrange
            var user = new IdentityUser { UserName = "invalidEmailUser", Email = "invalidemail" };
            var password = "ValidPassword123!";

            // Setup the user validator to return an error for an invalid email.
            _mockUserValidator
                .Setup(v => v.ValidateAsync(_userManager, user))
                .ReturnsAsync(IdentityResult.Failed(
                    new IdentityError { Description = "Invalid email format." }
                ));

            // Act
            var result = await _userManager.CreateAsync(user, password);

            // Assert
            Assert.That(result.Succeeded, Is.False, "Expected user creation to fail with an invalid email.");
            Assert.That(result.Errors, Has.One.Items, "Should have exactly one error.");
            Assert.That(result.Errors,
                Has.Some.Matches<IdentityError>(e => e.Description.Contains("Invalid email")),
                "Expected error message about invalid email.");
        }

        [Test]
        public async Task CreateUserAsync_NullEmail_ShouldFail()
        {
            // Arrange
            var user = new IdentityUser { UserName = "nullEmailUser", Email = null };
            var password = "ValidPassword123!";

            // Setup the user validator to return an error when email is null.
            _mockUserValidator
                .Setup(v => v.ValidateAsync(_userManager, user))
                .ReturnsAsync(IdentityResult.Failed(
                    new IdentityError { Description = "Email cannot be null." }
                ));

            // Act
            var result = await _userManager.CreateAsync(user, password);

            // Assert
            Assert.That(result.Succeeded, Is.False, "Expected user creation to fail with a null email.");
            Assert.That(result.Errors, Has.One.Items, "Should have exactly one error.");
            Assert.That(result.Errors,
                Has.Some.Matches<IdentityError>(e => e.Description.Contains("cannot be null")),
                "Expected error message about null email.");
        }

        #endregion
    }
}
