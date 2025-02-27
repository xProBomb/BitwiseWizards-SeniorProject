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
    private void SeedMultipleHoldings(int userId)
        {
            var user = new User 
            { 
                Id = userId,
                Username = $"testuser{userId}",
                Email = $"test{userId}@example.com",
                IdentityId = $"test_identity_{userId}",
                PasswordHash = "hashedpassword123",
                ProfileName = $"Test User {userId}",
                IsAdmin = false,
                IsVerified = false,
                PlaidEnabled = false,
                PlaidStatus = "Not Connected",
                CreatedAt = DateTime.UtcNow
            };

            var plaidConnection = new PlaidConnection 
            { 
                Id = userId,
                UserId = user.Id,
                ItemId = $"test_item_{userId}",
                AccessToken = $"test_token_{userId}",
                InstitutionId = "test_institution",
                InstitutionName = "Test Bank",
                LastSyncTimestamp = DateTime.UtcNow
            };

            var positions = new List<InvestmentPosition>
            {
                new InvestmentPosition
                {
                    Id = userId * 10 + 1,
                    PlaidConnectionId = plaidConnection.Id,
                    SecurityId = "AAPL",
                    Symbol = "AAPL",
                    Quantity = 10,
                    CostBasis = 150.00M,
                    CurrentPrice = 175.00M,
                    TypeOfSecurity = "equity",
                    LastUpdated = DateTime.UtcNow
                },
                new InvestmentPosition
                {
                    Id = userId * 10 + 2,
                    PlaidConnectionId = plaidConnection.Id,
                    SecurityId = "GOOGL",
                    Symbol = "GOOGL",
                    Quantity = 5,
                    CostBasis = 2500.00M,
                    CurrentPrice = 2600.00M,
                    TypeOfSecurity = "equity",
                    LastUpdated = DateTime.UtcNow
                },
                new InvestmentPosition
                {
                    Id = userId * 10 + 3,
                    PlaidConnectionId = plaidConnection.Id,
                    SecurityId = "BOND123",
                    Symbol = "BOND123",
                    Quantity = 100,
                    CostBasis = 100.00M,
                    CurrentPrice = 101.00M,
                    TypeOfSecurity = "fixed_income",
                    LastUpdated = DateTime.UtcNow
                }
            };

            _context.Users.Add(user);
            _context.PlaidConnections.Add(plaidConnection);
            _context.InvestmentPositions.AddRange(positions);
            _context.SaveChanges();
        }

        [Test]
        public async Task RemoveHoldingsForUserAsync_ExistingHoldings_RemovesSuccessfully()
        {
            // Arrange
            int userId = 2;
            SeedMultipleHoldings(userId);

            // Act
            var removedCount = await _repository.RemoveHoldingsForUserAsync(userId);
            var remainingHoldings = await _repository.GetHoldingsForUserAsync(userId);

            // Assert
            Assert.That(removedCount, Is.EqualTo(3)); // Should remove all 3 holdings
            Assert.That(remainingHoldings, Is.Empty);
        }

        [Test]
        public async Task RemoveHoldingsForUserAsync_NoHoldings_ReturnsZero()
        {
            // Arrange
            int userId = 999; // Non-existent user

            // Act
            var removedCount = await _repository.RemoveHoldingsForUserAsync(userId);

            // Assert
            Assert.That(removedCount, Is.EqualTo(0));
        }

        [Test]
        public async Task GetHoldingsForUserAsync_MultipleHoldings_ReturnsAllHoldings()
        {
            // Arrange
            int userId = 3;
            SeedMultipleHoldings(userId);

            // Act
            var holdings = await _repository.GetHoldingsForUserAsync(userId);

            // Assert
            Assert.That(holdings, Is.Not.Null);
            Assert.That(holdings.Count, Is.EqualTo(3));
            Assert.That(holdings.Select(h => h.Symbol), 
                       Does.Contain("AAPL")
                           .And.Contain("GOOGL")
                           .And.Contain("BOND123"));
            Assert.That(holdings.Select(h => h.TypeOfSecurity), 
                       Does.Contain("equity")
                           .And.Contain("fixed_income"));
        }

        [Test]
        public async Task GetHoldingsForUserAsync_AfterRemoval_VerifyCleanup()
        {
            // Arrange
            int userId = 4;
            SeedMultipleHoldings(userId);

            // Act
            await _repository.RemoveHoldingsForUserAsync(userId);
            var holdings = await _repository.GetHoldingsForUserAsync(userId);
            var plaidConnections = await _context.PlaidConnections
                .Where(pc => pc.UserId == userId)
                .ToListAsync();

            // Assert
            Assert.That(holdings, Is.Empty);
            Assert.That(plaidConnections, Is.Not.Empty, "PlaidConnection should not be removed");
            Assert.That(plaidConnections.First().InvestmentPositions, Is.Empty);
        }
    }
}