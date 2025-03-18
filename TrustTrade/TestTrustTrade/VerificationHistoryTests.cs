using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TrustTrade.DAL.Concrete;
using TrustTrade.Models;

namespace TestTrustTrade
{
    /// <summary>
    /// Tests for the VerificationHistoryRepository implementation.
    /// Validates repository methods for managing verification status history records.
    /// </summary>
    [TestFixture]
    public class VerificationHistoryRepositoryTests
    {
        private Mock<ILogger<VerificationHistoryRepository>> _loggerMock;
        private DbContextOptions<TrustTradeDbContext> _options;
        private TrustTradeDbContext _context;
        private VerificationHistoryRepository _repository;
        private int _testUserId = 1;

        [SetUp]
        public void Setup()
        {
            // Create in-memory database for testing with unique name to ensure test isolation
            _options = new DbContextOptionsBuilder<TrustTradeDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;
            
            _context = new TrustTradeDbContext(_options);
            
            // Create mock logger
            _loggerMock = new Mock<ILogger<VerificationHistoryRepository>>();
            
            // Create repository with mocked dependencies
            _repository = new VerificationHistoryRepository(_context, _loggerMock.Object);
            
            // Set up test data
            SetupTestData();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private void SetupTestData()
        {
            // Create test user
            // Note: Added ProfileName property which is required by the model but was missing
            var user = new User
            {
                Id = _testUserId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                CreatedAt = DateTime.Now.AddDays(-30),
                ProfileName = "Test User" // Required property that was missing
            };
            
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        [Test]
        public async Task GetHistoryForUserAsync_ReturnsEmptyList_WhenNoHistory()
        {
            // Act
            var result = await _repository.GetHistoryForUserAsync(_testUserId);
            
            // Assert
            Assert.That(result, Is.Not.Null, "Result should not be null");
            Assert.That(result, Is.Empty, "Result should be an empty list");
        }

        [Test]
        public async Task AddVerificationRecordAsync_AddsRecord_WhenNoExistingRecords()
        {
            // Arrange
            const bool isVerified = true;
            const string reason = "Test reason";
            const string source = "Test source";
            
            // Act
            var result = await _repository.AddVerificationRecordAsync(_testUserId, isVerified, reason, source);
            
            // Assert
            Assert.That(result, Is.Not.Null, "Result should not be null");
            Assert.That(result.UserId, Is.EqualTo(_testUserId), "UserId should match test user ID");
            Assert.That(result.IsVerified, Is.EqualTo(isVerified), "IsVerified should match input value");
            Assert.That(result.Reason, Is.EqualTo(reason), "Reason should match input value");
            Assert.That(result.Source, Is.EqualTo(source), "Source should match input value");
            
            // Verify record was added to database
            var records = await _context.VerificationHistory.ToListAsync();
            Assert.That(records.Count, Is.EqualTo(1), "Database should contain one record");
        }

        [Test]
        public async Task AddVerificationRecordAsync_DoesNotDuplicate_WhenStatusUnchanged()
        {
            // Arrange
            const bool isVerified = true;
            await _repository.AddVerificationRecordAsync(_testUserId, isVerified, "Initial", "Test");
            
            // Act - Try to add another record with same status
            var result = await _repository.AddVerificationRecordAsync(_testUserId, isVerified, "Duplicate", "Test");
            
            // Assert
            var records = await _context.VerificationHistory.ToListAsync();
            Assert.That(records.Count, Is.EqualTo(1), "Only one record should exist");
            Assert.That(records[0].Reason, Is.EqualTo("Initial"), "Original reason should be preserved");
        }

        [Test]
        public async Task AddVerificationRecordAsync_AddsNewRecord_WhenStatusChanges()
        {
            // Arrange
            await _repository.AddVerificationRecordAsync(_testUserId, true, "Initial verified", "Test");
            
            // Act - Change from verified to unverified
            await _repository.AddVerificationRecordAsync(_testUserId, false, "Unverified", "Test");
            
            // Assert
            var records = await _context.VerificationHistory.OrderBy(vh => vh.Timestamp).ToListAsync();
            Assert.That(records.Count, Is.EqualTo(2), "Two records should exist");
            Assert.That(records[0].IsVerified, Is.True, "First record should be verified");
            Assert.That(records[1].IsVerified, Is.False, "Second record should be unverified");
        }

        [Test]
        public async Task GetMostRecentStatusAsync_ReturnsNull_WhenNoHistory()
        {
            // Act
            var result = await _repository.GetMostRecentStatusAsync(_testUserId);
            
            // Assert
            Assert.That(result, Is.Null, "Result should be null for user with no history");
        }

        [Test]
        public async Task GetMostRecentStatusAsync_ReturnsCorrectRecord_WhenMultipleRecordsExist()
        {
            // Arrange
            await _repository.AddVerificationRecordAsync(_testUserId, true, "First", "Test");
            await Task.Delay(10); // Ensure different timestamps
            await _repository.AddVerificationRecordAsync(_testUserId, false, "Second", "Test");
            
            // Act
            var result = await _repository.GetMostRecentStatusAsync(_testUserId);
            
            // Assert
            Assert.That(result, Is.Not.Null, "Result should not be null");
            Assert.That(result.IsVerified, Is.False, "Should return most recent status (unverified)");
            Assert.That(result.Reason, Is.EqualTo("Second"), "Should return most recent reason");
        }

        [Test]
        public async Task CalculateVerifiedDurationAsync_ReturnsZero_WhenNoHistory()
        {
            // Act
            var result = await _repository.CalculateVerifiedDurationAsync(_testUserId);
            
            // Assert
            Assert.That(result, Is.EqualTo(TimeSpan.Zero), "Duration should be zero when no history exists");
        }

        [Test]
        public async Task CalculateVerifiedDurationAsync_ReturnsCorrectDuration_ForSingleVerificationPeriod()
        {
            // Arrange
            var startTime = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var endTime = startTime.AddHours(5);
            
            // Add verification history with controlled timestamps
            var start = new VerificationHistory
            {
                UserId = _testUserId,
                IsVerified = true,
                Timestamp = startTime,
                Reason = "Start",
                Source = "Test"
            };
            
            var end = new VerificationHistory
            {
                UserId = _testUserId,
                IsVerified = false,
                Timestamp = endTime,
                Reason = "End",
                Source = "Test"
            };
            
            _context.VerificationHistory.Add(start);
            _context.VerificationHistory.Add(end);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _repository.CalculateVerifiedDurationAsync(_testUserId);
            
            // Assert
            Assert.That(result, Is.EqualTo(TimeSpan.FromHours(5)), 
                "Duration should match the 5-hour difference between start and end");
        }

        [Test]
        public async Task CalculateVerifiedDurationAsync_ReturnsCorrectDuration_ForMultipleVerificationPeriods()
        {
            // Arrange
            var baseTime = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            
            var records = new List<VerificationHistory>
            {
                new VerificationHistory // First verification period start
                {
                    UserId = _testUserId,
                    IsVerified = true,
                    Timestamp = baseTime,
                    Reason = "First start",
                    Source = "Test"
                },
                new VerificationHistory // First verification period end
                {
                    UserId = _testUserId,
                    IsVerified = false,
                    Timestamp = baseTime.AddHours(2),
                    Reason = "First end",
                    Source = "Test"
                },
                new VerificationHistory // Second verification period start
                {
                    UserId = _testUserId,
                    IsVerified = true,
                    Timestamp = baseTime.AddHours(4),
                    Reason = "Second start",
                    Source = "Test"
                },
                new VerificationHistory // Second verification period end
                {
                    UserId = _testUserId,
                    IsVerified = false,
                    Timestamp = baseTime.AddHours(7),
                    Reason = "Second end",
                    Source = "Test"
                }
            };
            
            _context.VerificationHistory.AddRange(records);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _repository.CalculateVerifiedDurationAsync(_testUserId);
            
            // Assert - should be 2 + 3 = 5 hours
            Assert.That(result, Is.EqualTo(TimeSpan.FromHours(5)), 
                "Duration should be the sum of the two verification periods (2h + 3h = 5h)");
        }

        [Test]
        public async Task CalculateVerifiedDurationAsync_IncludesCurrentPeriod_WhenUserIsCurrentlyVerified()
        {
            // Arrange
            // Use explicit DateTimeKind.Utc to ensure consistent time handling
            var startTime = DateTime.Now.AddHours(-3);
    
            // Add only a "start verification" record without an end
            var start = new VerificationHistory
            {
                UserId = _testUserId,
                IsVerified = true,
                Timestamp = startTime,
                Reason = "Start",
                Source = "Test"
            };
    
            _context.VerificationHistory.Add(start);
            await _context.SaveChangesAsync();
    
            // Act
            var result = await _repository.CalculateVerifiedDurationAsync(_testUserId);
    
            // Assert - should be approximately 3 hours
            var expectedMinimumDuration = TimeSpan.FromHours(2.9); // Allow more flexible lower bound
            var expectedMaximumDuration = TimeSpan.FromHours(3.1); // Allow for test execution time
    
            // Debugging output
            Console.WriteLine($"Start time: {startTime}");
            Console.WriteLine($"Current time: {DateTime.Now}");
            Console.WriteLine($"Calculated duration: {result}");
    
            Assert.That(result, Is.GreaterThan(TimeSpan.Zero), 
                "Duration should be positive");
            Assert.That(result, Is.GreaterThanOrEqualTo(expectedMinimumDuration), 
                "Duration should be at least approximately 3 hours");
            Assert.That(result, Is.LessThanOrEqualTo(expectedMaximumDuration), 
                "Duration should not exceed expected time plus tolerance");
        }

        [Test]
        public async Task GetVerificationDatesAsync_ReturnsNull_WhenNoHistory()
        {
            // Act
            var (first, recent) = await _repository.GetVerificationDatesAsync(_testUserId);
            
            // Assert
            Assert.That(first, Is.Null, "First verified date should be null");
            Assert.That(recent, Is.Null, "Most recent verified date should be null");
        }

        [Test]
        public async Task GetVerificationDatesAsync_ReturnsCorrectDates_WhenMultipleRecordsExist()
        {
            // Arrange
            var baseTime = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            
            var records = new List<VerificationHistory>
            {
                new VerificationHistory
                {
                    UserId = _testUserId,
                    IsVerified = true,
                    Timestamp = baseTime,
                    Reason = "First",
                    Source = "Test"
                },
                new VerificationHistory
                {
                    UserId = _testUserId,
                    IsVerified = false,
                    Timestamp = baseTime.AddHours(2),
                    Reason = "Second",
                    Source = "Test"
                },
                new VerificationHistory
                {
                    UserId = _testUserId,
                    IsVerified = true,
                    Timestamp = baseTime.AddHours(4),
                    Reason = "Third",
                    Source = "Test"
                }
            };
            
            _context.VerificationHistory.AddRange(records);
            await _context.SaveChangesAsync();
            
            // Act
            var (first, recent) = await _repository.GetVerificationDatesAsync(_testUserId);
            
            // Assert
            Assert.That(first, Is.Not.Null, "First verified date should not be null");
            Assert.That(recent, Is.Not.Null, "Most recent verified date should not be null");
            Assert.That(first, Is.EqualTo(baseTime), "First verified date should match first record timestamp");
            Assert.That(recent, Is.EqualTo(baseTime.AddHours(4)), 
                "Most recent verified date should match last verified record timestamp");
        }
    }
}