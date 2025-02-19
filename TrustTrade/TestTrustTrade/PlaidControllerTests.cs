/*using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System.Security.Claims;
using TrustTrade.Controllers.Api;
using TrustTrade.Models;
using Going.Plaid;
using Going.Plaid.Entity;
using Going.Plaid.Link;
using Going.Plaid.Item;
using Going.Plaid.Institutions;
using Microsoft.Extensions.Logging;

namespace TestTrustTrade
{
    [TestFixture]
    public class PlaidControllerTests
    {
        private Mock<PlaidClient> _plaidClientMock;
        private Mock<UserManager<IdentityUser>> _userManagerMock;
        private Mock<TrustTradeDbContext> _dbContextMock;
        private Mock<ILogger<PlaidController>> _loggerMock;
        private PlaidController _controller;
        private Mock<DbSet<User>> _userDbSetMock;
        private Mock<DbSet<PlaidConnection>> _plaidConnectionDbSetMock;

        [SetUp]
        public void Setup()
        {
            // Setup PlaidClient mock
            _plaidClientMock = new Mock<PlaidClient>(MockBehavior.Strict);

            // Setup UserManager mock (requires custom setup due to abstract class)
            var userStore = new Mock<IUserStore<IdentityUser>>();
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            // Setup DbContext and Logger mocks
            _dbContextMock = new Mock<TrustTradeDbContext>();
            _loggerMock = new Mock<ILogger<PlaidController>>();
            _userDbSetMock = new Mock<DbSet<User>>();
            _plaidConnectionDbSetMock = new Mock<DbSet<PlaidConnection>>();

            // Setup controller
            _controller = new PlaidController(
                _plaidClientMock.Object,
                _userManagerMock.Object,
                _dbContextMock.Object,
                _loggerMock.Object
            );
        }

        [Test]
        public async Task CreateLinkToken_AuthenticatedUser_ReturnsLinkToken()
        {
            // Arrange
            var identityUser = new IdentityUser { Id = "test_id", Email = "test@example.com" };
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(identityUser);

            var expectedLinkToken = "link-sandbox-123";
            _plaidClientMock.Setup(x => x.LinkTokenCreateAsync(It.IsAny<LinkTokenCreateRequest>()))
                .ReturnsAsync(new LinkTokenCreateResponse { LinkToken = expectedLinkToken });

            // Act
            var result = await _controller.CreateLinkToken();

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            var response = okResult.Value as LinkTokenResponse;
            Assert.That(response.LinkToken, Is.EqualTo(expectedLinkToken));
        }

        [Test]
        public async Task CreateLinkToken_UnauthorizedUser_ReturnsUnauthorized()
        {
            // Arrange
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((IdentityUser)null);

            // Act
            var result = await _controller.CreateLinkToken();

            // Assert
            Assert.That(result.Result, Is.InstanceOf<UnauthorizedResult>());
        }

        [Test]
        public async Task ExchangePublicToken_ValidToken_CreatesPlaidConnection()
        {
            // Arrange
            var identityUser = new IdentityUser { Id = "test_id", Email = "test@example.com" };
            var trustTradeUser = new User 
            { 
                Id = 1, 
                Email = "test@example.com",
                Username = "testuser",
                ProfileName = "Test User",
                PasswordHash = "hash"
            };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(identityUser);

            var users = new List<User> { trustTradeUser }.AsQueryable();
            SetupDbSet(_userDbSetMock, users);
            _dbContextMock.Setup(x => x.Users).Returns(_userDbSetMock.Object);

            _plaidClientMock.Setup(x => x.ItemPublicTokenExchangeAsync(It.IsAny<ItemPublicTokenExchangeRequest>()))
                .ReturnsAsync(new ItemPublicTokenExchangeResponse 
                { 
                    AccessToken = "access-test-123",
                    ItemId = "item-test-123"
                });

            _plaidClientMock.Setup(x => x.ItemGetAsync(It.IsAny<ItemGetRequest>()))
                .ReturnsAsync(new ItemGetResponse 
                { 
                    Item = new Item { InstitutionId = "ins_123" }
                });

            _plaidClientMock.Setup(x => x.InstitutionsGetByIdAsync(It.IsAny<InstitutionsGetByIdRequest>()))
                .ReturnsAsync(new InstitutionsGetByIdResponse 
                { 
                    Institution = new Institution { Name = "Test Bank" }
                });

            // Act
            var result = await _controller.ExchangePublicToken(new PlaidController.ExchangePublicTokenRequest 
            { 
                PublicToken = "public-test-123" 
            });

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        private void SetupDbSet<T>(Mock<DbSet<T>> mockSet, IQueryable<T> data) where T : class
        {
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        }
    }
}*/