using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrustTrade.Data;
using TrustTrade.Models;

namespace TestTrustTrade
{
    [TestFixture]
    public class TrustTradeDbContextTests
    {
        private DbContextOptions<TrustTradeDbContext> _options;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<TrustTradeDbContext>()
                .UseInMemoryDatabase(databaseName: $"TrustTradeTestDb_{Guid.NewGuid()}")
                .Options;
        }

        [Test]
        public async Task CanRetrieveUserWithPosts()
        {
            // Arrange
            using (var context = new TrustTradeDbContext(_options))
            {
                var user = new User { Id = 1, Username = "testuser", Email = "test@example.com", PasswordHash = "testhash", ProfileName = "Test User" };
                context.Users.Add(user);

                context.Posts.AddRange(
                    new Post { Id = 1, Title = "First Post", UserId = 1, Content = "Content of the first post" },
                    new Post { Id = 2, Title = "Second Post", UserId = 1, Content = "Content of the second post" }
                );

                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new TrustTradeDbContext(_options))
            {
                var user = await context.Users
                    .Include(u => u.Posts)
                    .FirstOrDefaultAsync(u => u.Username == "testuser");

                // Assert
                Assert.That(user != null);
                Assert.That(2 == user.Posts.Count);
                Assert.That(user.Posts.Any(p => p.Title == "First Post"));
                Assert.That(user.Posts.Any(p => p.Title == "Second Post"));
            }
        }

        [Test]
        public async Task CanRetrievePostWithComments()
        {
            // Arrange
            using (var context = new TrustTradeDbContext(_options))
            {
                var user = new User { Id = 1, Username = "testuser", Email = "test@example.com", PasswordHash = "testhash", ProfileName = "Test User" };
                context.Users.Add(user);

                var post = new Post { Id = 1, Title = "Test Post", UserId = 1, Content = "Test Content" };
                context.Posts.Add(post);

                context.Comments.AddRange(
                    new Comment { Id = 1, Content = "First comment", PostId = 1, UserId = 1 },
                    new Comment { Id = 2, Content = "Second comment", PostId = 1, UserId = 1 }
                );

                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new TrustTradeDbContext(_options))
            {
                var post = await context.Posts
                    .Include(p => p.Comments)
                    .FirstOrDefaultAsync(p => p.Id == 1);

                // Assert
                Assert.That(post != null);
                Assert.That(2 == post.Comments.Count);
                Assert.That(post.Comments.Any(c => c.Content == "First comment"));
                Assert.That(post.Comments.Any(c => c.Content == "Second comment"));
            }
        }

        [Test]
        public async Task CanRetrieveUserWithFollowers()
        {
            // Arrange
            using (var context = new TrustTradeDbContext(_options))
            {
                context.Users.AddRange(
                    new User { Id = 1, Username = "user1", Email = "user1@example.com", PasswordHash = "hash1", ProfileName = "User One", IsVerified = true },
                    new User { Id = 2, Username = "user2", Email = "user2@example.com", PasswordHash = "hash2", ProfileName = "User Two", IsVerified = true },
                    new User { Id = 3, Username = "user3", Email = "user3@example.com", PasswordHash = "hash3", ProfileName = "User Three", IsVerified = false }
                );

                context.Followers.AddRange(
                    new Follower { Id = 1, FollowerUserId = 2, FollowingUserId = 1 },
                    new Follower { Id = 2, FollowerUserId = 3, FollowingUserId = 1 }
                );

                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new TrustTradeDbContext(_options))
            {
                var user = await context.Users
                    .Include(u => u.FollowerFollowingUsers)
                    .ThenInclude(f => f.FollowerUser)
                    .FirstOrDefaultAsync(u => u.Id == 1);

                // Assert
                Assert.That(user != null);
                Assert.That(2 == user.FollowerFollowingUsers.Count);
            }
        }

        [Test]
        public async Task CanAssociateTagsWithPost()
        {
            // Arrange
            using (var context = new TrustTradeDbContext(_options))
            {
                var user = new User { Id = 1, Username = "testuser", Email = "test@example.com", PasswordHash = "testhash", ProfileName = "testprofile" };
                context.Users.Add(user);

                var post = new Post { Id = 1, Title = "Investment Strategy", UserId = 1, Content = "Sample content" };

                var tags = new List<Tag>
                {
                    new Tag { Id = 1, TagName = "Finance" },
                    new Tag { Id = 2, TagName = "Investing" }
                };

                context.Tags.AddRange(tags);
                await context.SaveChangesAsync();

                post.Tags = tags;
                context.Posts.Add(post);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new TrustTradeDbContext(_options))
            {
                var post = await context.Posts
                    .Include(p => p.Tags)
                    .FirstOrDefaultAsync(p => p.Id == 1);

                // Assert
                Assert.That(post != null);
                Assert.That(2 == post.Tags.Count);
                Assert.That(post.Tags.Any(t => t.TagName == "Finance"));
                Assert.That(post.Tags.Any(t => t.TagName == "Investing"));
            }
        }

        [Test]
        public async Task CanUpdateExistingEntity()
        {
            // Arrange
            using (var context = new TrustTradeDbContext(_options))
            {
                var user = new User { Id = 1, Username = "testuser", Email = "test@example.com", PasswordHash = "testhash", ProfileName = "testprofile" };
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new TrustTradeDbContext(_options))
            {
                var user = await context.Users.FindAsync(1);
                user.Bio = "This is my updated bio";
                user.ProfileName = "Updated Name";
                await context.SaveChangesAsync();
            }

            // Assert
            using (var context = new TrustTradeDbContext(_options))
            {
                var user = await context.Users.FindAsync(1);
                Assert.That(user != null);
                Assert.That("This is my updated bio" == user.Bio);
                Assert.That("Updated Name" == user.ProfileName);
            }
        }

        [Test]
        public async Task CanDeleteEntity()
        {
            // Arrange
            using (var context = new TrustTradeDbContext(_options))
            {
                var user = new User { Id = 1, Username = "testuser", Email = "test@example.com", PasswordHash = "testhash", ProfileName = "Test User" };
                context.Users.Add(user);

                var post = new Post { Id = 1, Title = "Test Post", UserId = 1, Content = "Test Content" };
                context.Posts.Add(post);

                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new TrustTradeDbContext(_options))
            {
                var post = await context.Posts.FindAsync(1);
                context.Posts.Remove(post);
                await context.SaveChangesAsync();
            }

            // Assert
            using (var context = new TrustTradeDbContext(_options))
            {
                var postExists = await context.Posts.AnyAsync(p => p.Id == 1);
                Assert.That(postExists != true);

                // User should still exist
                var userExists = await context.Users.AnyAsync(u => u.Id == 1);
                Assert.That(userExists);
            }
        }

        [Test]
        public async Task CanQueryWithFiltering()
        {
            // Arrange
            using (var context = new TrustTradeDbContext(_options))
            {
                context.Users.AddRange(
                    new User { Id = 1, Username = "user1", Email = "user1@example.com", PasswordHash = "hash1", ProfileName = "User One", IsVerified = true },
                    new User { Id = 2, Username = "user2", Email = "user2@example.com", PasswordHash = "hash2", ProfileName = "User Two", IsVerified = true },
                    new User { Id = 3, Username = "user3", Email = "user3@example.com", PasswordHash = "hash3", ProfileName = "User Three", IsVerified = false }
                );

                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new TrustTradeDbContext(_options))
            {
                var verifiedUsers = await context.Users
                    .Where(u => u.IsVerified == true)
                    .ToListAsync();

                // Assert
                Assert.That(2 == verifiedUsers.Count);
                Assert.That(verifiedUsers.All(u => (bool)u.IsVerified));
            }
        }

        [Test]
        public async Task CanPerformComplexQueries()
        {
            // Arrange
            using (var context = new TrustTradeDbContext(_options))
            {
                // Add users
                context.Users.AddRange(
                    new User { Id = 1, Username = "trader1", Email = "trader1@example.com", PasswordHash = "hash1", ProfileName = "Trader One" },
                    new User { Id = 2, Username = "trader2", Email = "trader2@example.com", PasswordHash = "hash2", ProfileName = "Trader Two" }
                );

                // Add stocks
                context.Stocks.AddRange(
                    new Stock { Id = 1, TickerSymbol = "AAPL", StockPrice = 150.00M },
                    new Stock { Id = 2, TickerSymbol = "GOOGL", StockPrice = 2500.00M },
                    new Stock { Id = 3, TickerSymbol = "MSFT", StockPrice = 300.00M }
                );

                // Add trades
                context.Trades.AddRange(
                    new Trade { Id = 1, UserId = 1, TickerSymbol = "AAPL", Quantity = 10, EntryPrice = 145.00M, TradeType = "Buy" },
                    new Trade { Id = 2, UserId = 1, TickerSymbol = "GOOGL", Quantity = 5, EntryPrice = 2400.00M, TradeType = "Buy" },
                    new Trade { Id = 3, UserId = 2, TickerSymbol = "MSFT", Quantity = 8, EntryPrice = 290.00M, TradeType = "Buy" },
                    new Trade { Id = 4, UserId = 2, TickerSymbol = "AAPL", Quantity = 15, EntryPrice = 148.00M, TradeType = "Buy" }
                );

                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new TrustTradeDbContext(_options))
            {
                // Find users who have Apple stocks
                var appleStockHolders = await context.Users
                    .Where(u => u.Trades.Any(t => t.TickerSymbol == "AAPL"))
                    .ToListAsync();

                // Find total quantity of each stock
                var stockQuantities = await context.Trades
                    .GroupBy(t => t.TickerSymbol)
                    .Select(g => new { Symbol = g.Key, TotalQuantity = g.Sum(t => t.Quantity) })
                    .ToListAsync();

                // Find stocks trading above $200
                var expensiveStocks = await context.Stocks
                    .Where(s => s.StockPrice > 200)
                    .ToListAsync();

                // Assert
                Assert.That(2 == appleStockHolders.Count);

                var aaplQuantity = stockQuantities.FirstOrDefault(s => s.Symbol == "AAPL");
                Assert.That(aaplQuantity != null);
                Assert.That(25 == aaplQuantity.TotalQuantity);

                Assert.That(2 == expensiveStocks.Count);
                Assert.That(expensiveStocks.All(s => s.StockPrice > 200));
            }
        }

        [Test]
        public async Task CanTrackChanges()
        {
            // Arrange
            using (var context = new TrustTradeDbContext(_options))
            {
                var stock = new Stock { Id = 1, TickerSymbol = "NFLX", StockPrice = 500.00M };
                context.Stocks.Add(stock);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new TrustTradeDbContext(_options))
            {
                // Get the entity and modify it
                var stock = await context.Stocks.FindAsync(1);
                Assert.That(EntityState.Unchanged == context.Entry(stock).State);

                // Modify the entity
                stock.StockPrice = 550.00M;
                Assert.That(EntityState.Modified == context.Entry(stock).State);

                // Only specific properties modified
                Assert.That(context.Entry(stock).Property(s => s.StockPrice).IsModified);
                Assert.That(!context.Entry(stock).Property(s => s.TickerSymbol).IsModified);

                // Save changes
                await context.SaveChangesAsync();
                Assert.That(EntityState.Unchanged == context.Entry(stock).State);
            }

            // Verify
            using (var context = new TrustTradeDbContext(_options))
            {
                var stock = await context.Stocks.FindAsync(1);
                Assert.That(550.00M == stock.StockPrice);
            }
        }
    }
}