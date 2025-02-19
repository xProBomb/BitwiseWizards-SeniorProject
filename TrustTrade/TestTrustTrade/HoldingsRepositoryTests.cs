using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TrustTrade.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using Moq;
using TrustTrade.DAL.Concrete;

namespace TestTrustTrade
{
    [TestFixture]
    public class HoldingsRepositoryTests
    {
        private TrustTradeDbContext _context;
        private HoldingsRepository _repository;
        private Mock<ILogger<HoldingsRepository>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<TrustTradeDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + System.Guid.NewGuid())
                .Options;
            
            _context = new TrustTradeDbContext(options);
            _loggerMock = new Mock<ILogger<HoldingsRepository>>();
            
            // Using null! to suppress nullable warning for PlaidClient
            _repository = new HoldingsRepository(_context, null!, _loggerMock.Object);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var user = new User 
            { 
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                IdentityId = "test_identity",
                PasswordHash = "hashedpassword123", // Required field
                ProfileName = "Test User",          // Required field
                IsAdmin = false,
                IsVerified = false,
                PlaidEnabled = false,
                PlaidStatus = "Not Connected",
                CreatedAt = DateTime.UtcNow
            };

            var plaidConnection = new PlaidConnection 
            { 
                Id = 1,
                UserId = user.Id,
                ItemId = "test_item",
                AccessToken = "test_token",
                InstitutionId = "test_institution",
                InstitutionName = "Test Bank",
                LastSyncTimestamp = DateTime.UtcNow
            };

            var position = new InvestmentPosition
            {
                Id = 1,
                PlaidConnectionId = plaidConnection.Id,
                SecurityId = "AAPL",
                Symbol = "AAPL",
                Quantity = 10,
                CostBasis = 150.00M,
                CurrentPrice = 175.00M,
                TypeOfSecurity = "equity",
                LastUpdated = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.PlaidConnections.Add(plaidConnection);
            _context.InvestmentPositions.Add(position);
            _context.SaveChanges();
        }

        [Test]
        public async Task GetHoldingsForUserAsync_UserWithHoldings_ReturnsHoldings()
        {
            // Arrange
            int userId = 1;

            // Act
            var holdings = await _repository.GetHoldingsForUserAsync(userId);

            // Assert
            Assert.That(holdings, Is.Not.Null);
            Assert.That(holdings.Count, Is.EqualTo(1));
            Assert.That(holdings[0].Symbol, Is.EqualTo("AAPL"));
            Assert.That(holdings[0].TypeOfSecurity, Is.EqualTo("equity"));
            Assert.That(holdings[0].Quantity, Is.EqualTo(10));
            Assert.That(holdings[0].CostBasis, Is.EqualTo(150.00M));
        }

        [Test]
        public async Task GetHoldingsForUserAsync_UserWithNoHoldings_ReturnsEmptyList()
        {
            // Arrange
            int userId = 999; // Non-existent user

            // Act
            var holdings = await _repository.GetHoldingsForUserAsync(userId);

            // Assert
            Assert.That(holdings, Is.Empty);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}