using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TrustTrade.Data;
using TrustTrade.Models;

namespace TrustTrade.Tests.Repositories
{
    [TestFixture]
    public class UserRepositoryTests
    {
        private TrustTradeDbContext _context;
        private SearchUserRepository _repository;

        [SetUp]
        public void SetUp()
        {
            // Create a new in-memory database for each test
            var options = new DbContextOptionsBuilder<TrustTradeDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TrustTradeDbContext(options);

            // Seed sample users with required fields
            _context.Users.AddRange(new List<User>
            {
                new User 
                { 
                    Id = 1, 
                    Username = "johnDoe", 
                    ProfileName = "johnDoe", 
                    Email = "john@example.com", 
                    PasswordHash = "dummyHash", 
                    ProfilePicture = null,
                    CreatedAt = DateTime.Now.AddDays(-10) 
                },
                new User 
                { 
                    Id = 2, 
                    Username = "janeSmith", 
                    ProfileName = "janeSmith", 
                    Email = "jane@example.com", 
                    PasswordHash = "dummyHash", 
                    ProfilePicture = null, 
                    CreatedAt = DateTime.Now.AddDays(-20) 
                },
                new User 
                { 
                    Id = 3, 
                    Username = "johnSmith", 
                    ProfileName = "johnSmith", 
                    Email = "johnsmith@example.com", 
                    PasswordHash = "dummyHash", 
                    ProfilePicture = null, 
                    CreatedAt = DateTime.Now.AddDays(-5) 
                }
            });
            _context.SaveChanges();

            _repository = new SearchUserRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task SearchUsersAsync_WithValidSearchTerm_ReturnsMatchingUsers()
        {
            // Arrange
            string searchTerm = "john";

            // Act
            var result = await _repository.SearchUsersAsync(searchTerm);

            // Assert
            Assert.That(result, Is.Not.Null);
            // Expect 2 users: "johnDoe" and "johnSmith"
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.All(u => u.Username.IndexOf("john", StringComparison.OrdinalIgnoreCase) >= 0), Is.True);
        }

        [Test]
        public async Task SearchUsersAsync_WithEmptySearchTerm_ReturnsEmptyList()
        {
            // Arrange
            string searchTerm = "";

            // Act
            var result = await _repository.SearchUsersAsync(searchTerm);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task SearchUsersAsync_WithNullSearchTerm_ReturnsEmptyList()
        {
            // Arrange
            string searchTerm = null;

            // Act
            var result = await _repository.SearchUsersAsync(searchTerm);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task SearchUsersAsync_WithWhiteSpaceSearchTerm_ReturnsEmptyList()
        {
            // Arrange
            string searchTerm = "   ";

            // Act
            var result = await _repository.SearchUsersAsync(searchTerm);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task SearchUsersAsync_WithNoMatchingSearchTerm_ReturnsEmptyList()
        {
            // Arrange
            string searchTerm = "nonexistent";

            // Act
            var result = await _repository.SearchUsersAsync(searchTerm);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }
    }
}
