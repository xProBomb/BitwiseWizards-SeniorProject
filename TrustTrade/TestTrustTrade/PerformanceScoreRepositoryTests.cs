// File: TestTrustTrade/PerformanceScoreRepositoryTests.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TrustTrade.DAL.Abstract;
using TrustTrade.DAL.Concrete;
using TrustTrade.Models;

namespace TestTrustTrade
{
    [TestFixture]
    public class PerformanceScoreRepositoryTests
    {
        // Mock dependencies
        private Mock<IHoldingsRepository> _mockHoldingsRepository;
        private Mock<ILogger<PerformanceScoreRepository>> _mockLogger;
        private Mock<DbSet<User>> _mockUserDbSet;
        
        // System under test
        private PerformanceScoreRepository _repository;
        private Mock<TrustTradeDbContext> _contextMock;
        private TrustTradeDbContext _dbContext;
        
        [SetUp]
        public void Setup()
        {
            // Create mocks
            _mockHoldingsRepository = new Mock<IHoldingsRepository>();
            _mockLogger = new Mock<ILogger<PerformanceScoreRepository>>();
            _contextMock = new Mock<TrustTradeDbContext>();
            
            // Initialize empty user set
            SetupMockUserDbSet(new List<User>());
            
            // Create repository with mocked dependencies
            _repository = new PerformanceScoreRepository(
                _dbContext,
                _mockHoldingsRepository.Object,
                _mockLogger.Object
            );
        }
        
        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
        }
        
        #region MeetsMinimumRequirements Tests
        
        [Test]
        public void MeetsMinimumRequirements_WhenUserHasAllRequirements_ReturnsTrue()
        {
            // Arrange
            var user = new User
            {
                PlaidEnabled = true,
                Posts = new List<Post> { new Post() },
                PlaidConnections = new List<PlaidConnection>
                {
                    new PlaidConnection
                    {
                        InvestmentPositions = new List<InvestmentPosition> { new InvestmentPosition() }
                    }
                }
            };
            
            // Act
            var result = _repository.MeetsMinimumRequirements(user);
            
            // Assert
            Assert.That(result, Is.True, "User with all requirements should pass the minimum requirements check");
        }
        
        [Test]
        public void MeetsMinimumRequirements_WhenUserHasNoPlaid_ReturnsFalse()
        {
            // Arrange
            var user = new User
            {
                PlaidEnabled = false, // Missing Plaid
                Posts = new List<Post> { new Post() },
                PlaidConnections = new List<PlaidConnection>
                {
                    new PlaidConnection
                    {
                        InvestmentPositions = new List<InvestmentPosition> { new InvestmentPosition() }
                    }
                }
            };
            
            // Act
            var result = _repository.MeetsMinimumRequirements(user);
            
            // Assert
            Assert.That(result, Is.False, "User without Plaid enabled should not meet minimum requirements");
        }
        
        [Test]
        public void MeetsMinimumRequirements_WhenUserHasNoPosts_ReturnsFalse()
        {
            // Arrange
            var user = new User
            {
                PlaidEnabled = true,
                Posts = new List<Post>(), // No posts
                PlaidConnections = new List<PlaidConnection>
                {
                    new PlaidConnection
                    {
                        InvestmentPositions = new List<InvestmentPosition> { new InvestmentPosition() }
                    }
                }
            };
            
            // Act
            var result = _repository.MeetsMinimumRequirements(user);
            
            // Assert
            Assert.That(result, Is.False, "User without posts should not meet minimum requirements");
        }
        
        [Test]
        public void MeetsMinimumRequirements_WhenUserHasNoHoldings_ReturnsFalse()
        {
            // Arrange
            var user = new User
            {
                PlaidEnabled = true,
                Posts = new List<Post> { new Post() },
                PlaidConnections = new List<PlaidConnection>
                {
                    new PlaidConnection
                    {
                        InvestmentPositions = new List<InvestmentPosition>() // No holdings
                    }
                }
            };
            
            // Act
            var result = _repository.MeetsMinimumRequirements(user);
            
            // Assert
            Assert.That(result, Is.False, "User without holdings should not meet minimum requirements");
        }
        
        #endregion
        
        #region CalculatePerformanceScoreAsync Tests
        
        [Test]
        public async Task CalculatePerformanceScoreAsync_WhenUserDoesNotExist_ReturnsUnratedScore()
        {
            // Arrange
            int nonExistentUserId = 999;
            SetupMockUserDbSet(new List<User>()); // Empty user database
            
            // Act
            var (score, isRated, breakdown) = await _repository.CalculatePerformanceScoreAsync(nonExistentUserId);
            
            // Assert
            Assert.Multiple(() => {
                Assert.That(isRated, Is.False, "Non-existent user should not be rated");
                Assert.That(score, Is.EqualTo(0), "Score should be zero for non-existent user");
                Assert.That(breakdown, Is.Empty, "Breakdown should be empty for non-existent user");
            });
        }
        
        [Test]
        public async Task CalculatePerformanceScoreAsync_WhenUserDoesNotMeetRequirements_ReturnsUnratedScore()
        {
            // Arrange
            int userId = 1;
            var user = new User
            {
                Id = userId,
                PlaidEnabled = false, // Does not meet requirements
                Posts = new List<Post>(),
                PlaidConnections = new List<PlaidConnection>()
            };
            
            SetupMockUserDbSet(new List<User> { user });
            
            // Act
            var (score, isRated, breakdown) = await _repository.CalculatePerformanceScoreAsync(userId);
            
            // Assert
            Assert.Multiple(() => {
                Assert.That(isRated, Is.False, "User not meeting requirements should not be rated");
                Assert.That(score, Is.EqualTo(0), "Score should be zero for unrated user");
                Assert.That(breakdown, Is.Empty, "Breakdown should be empty for unrated user");
            });
        }
        
        [Test]
        public async Task CalculatePerformanceScoreAsync_WithAllPositiveMetrics_ReturnsHighScore()
        {
            // Arrange
            int userId = 1;
            var user = CreateUserWithRequirements(userId);
            
            SetupMockUserDbSet(new List<User> { user });
            
            // Setup holdings with all green (profitable) positions
            var holdings = new List<InvestmentPosition>
            {
                CreateHolding(100, 150, 10), // Cost 100, now worth 150
                CreateHolding(200, 300, 5),  // Cost 200, now worth 300
                CreateHolding(50, 80, 20)    // Cost 50, now worth 80
            };
            
            _mockHoldingsRepository.Setup(h => h.GetHoldingsForUserAsync(userId))
                .ReturnsAsync(holdings);
            
            // Act
            var (score, isRated, breakdown) = await _repository.CalculatePerformanceScoreAsync(userId);
            
            // Assert
            Assert.Multiple(() => {
                Assert.That(isRated, Is.True, "User should be rated");
                Assert.That(score, Is.GreaterThan(60), "Score should be high with all positive metrics");
                Assert.That(breakdown.Count, Is.EqualTo(3), "Breakdown should have three components");
                Assert.That(breakdown, Does.ContainKey("Holdings in Green"), "Should include holdings component");
                Assert.That(breakdown["Holdings in Green"], Is.GreaterThan(60), "Holdings score should be high");
            });
        }
        
        [Test]
        public async Task CalculatePerformanceScoreAsync_WithAllNegativeMetrics_ReturnsLowScore()
        {
            // Arrange
            int userId = 1;
            var user = CreateUserWithRequirements(userId);
            
            // Add a post with portfolio value higher than current value
            user.Posts.ElementAt(0).PortfolioValueAtPosting = 10000;
            
            SetupMockUserDbSet(new List<User> { user });
            
            // Setup holdings with all red (unprofitable) positions
            var holdings = new List<InvestmentPosition>
            {
                CreateHolding(150, 100, 10), // Cost 150, now worth 100
                CreateHolding(300, 200, 5),  // Cost 300, now worth 200
                CreateHolding(80, 50, 20)    // Cost 80, now worth 50
            };
            
            _mockHoldingsRepository.Setup(h => h.GetHoldingsForUserAsync(userId))
                .ReturnsAsync(holdings);
            
            // Act
            var (score, isRated, breakdown) = await _repository.CalculatePerformanceScoreAsync(userId);
            
            // Assert
            Assert.Multiple(() => {
                Assert.That(isRated, Is.True, "User should be rated");
                Assert.That(score, Is.LessThan(50), "Score should be low with all negative metrics");
                Assert.That(breakdown.Count, Is.EqualTo(3), "Breakdown should have three components");
                Assert.That(breakdown, Does.ContainKey("Holdings in Green"), "Should include holdings component");
                Assert.That(breakdown["Holdings in Green"], Is.LessThan(30), "Holdings score should be low");
            });
        }
        
        [Test]
        public async Task CalculatePerformanceScoreAsync_WithMixedMetrics_ReturnsMediumScore()
        {
            // Arrange
            int userId = 1;
            var user = CreateUserWithRequirements(userId);
            
            // Add a post with portfolio value slightly lower than current
            user.Posts.ElementAt(0).PortfolioValueAtPosting = 9000;
            
            SetupMockUserDbSet(new List<User> { user });
            
            // Setup holdings with mixed performance
            var holdings = new List<InvestmentPosition>
            {
                CreateHolding(100, 120, 10), // Profitable
                CreateHolding(200, 180, 10), // Unprofitable
                CreateHolding(150, 150, 10)  // Break-even
            };
            
            _mockHoldingsRepository.Setup(h => h.GetHoldingsForUserAsync(userId))
                .ReturnsAsync(holdings);
            
            // Act
            var (score, isRated, breakdown) = await _repository.CalculatePerformanceScoreAsync(userId);
            
            // Assert
            Assert.Multiple(() => {
                Assert.That(isRated, Is.True, "User should be rated");
                Assert.That(score, Is.InRange(30, 60), "Score should be medium with mixed metrics");
                Assert.That(breakdown.Count, Is.EqualTo(3), "Breakdown should have three components");
            });
        }
        
        [Test]
        public async Task CalculatePerformanceScoreAsync_WhenHoldingsRepositoryThrows_HandlesGracefully()
        {
            // Arrange
            int userId = 1;
            var user = CreateUserWithRequirements(userId);
            
            SetupMockUserDbSet(new List<User> { user });
            
            // Setup holdings repository to throw an exception
            _mockHoldingsRepository.Setup(h => h.GetHoldingsForUserAsync(userId))
                .ThrowsAsync(new Exception("Simulated error"));
            
            // Act
            var (score, isRated, breakdown) = await _repository.CalculatePerformanceScoreAsync(userId);
            
            // Assert
            Assert.Multiple(() => {
                Assert.That(isRated, Is.False, "User should not be rated when an error occurs");
                Assert.That(score, Is.EqualTo(0), "Score should be zero when an error occurs");
                Assert.That(breakdown, Is.Empty, "Breakdown should be empty when an error occurs");
            });
            
            // Verify that the error was logged
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error, 
                    It.IsAny<EventId>(), 
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        
        #endregion
        
        #region Helper Methods
        
        private User CreateUserWithRequirements(int userId)
        {
            return new User
            {
                Id = userId,
                PlaidEnabled = true,
                Posts = new List<Post> { new Post() },
                PlaidConnections = new List<PlaidConnection>
                {
                    new PlaidConnection
                    {
                        InvestmentPositions = new List<InvestmentPosition> { new InvestmentPosition() }
                    }
                }
            };
        }
        
        private InvestmentPosition CreateHolding(decimal costBasis, decimal currentPrice, decimal quantity)
        {
            return new InvestmentPosition
            {
                CostBasis = costBasis,
                CurrentPrice = currentPrice,
                Quantity = quantity
            };
        }
        
        private void SetupMockUserDbSet(List<User> users)
        {
            var queryable = users.AsQueryable();
            _mockUserDbSet = CreateMockDbSet(queryable);
            _contextMock.Setup(c => c.Users).Returns(_mockUserDbSet.Object);
            _dbContext = _contextMock.Object;
        }
        
        private Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(data.Provider));
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            mockSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));

            return mockSet;
        }
        
        #endregion
        
        #region Async Testing Infrastructure
        
        private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            public TestAsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                return new TestAsyncEnumerable<TEntity>(expression);
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                return new TestAsyncEnumerable<TElement>(expression);
            }

            public object Execute(Expression expression)
            {
                return _inner.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return _inner.Execute<TResult>(expression);
            }

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
            {
                var resultType = typeof(TResult).GetGenericArguments()[0];
                var executionResult = typeof(IQueryProvider)
                    .GetMethod(
                        name: nameof(IQueryProvider.Execute),
                        genericParameterCount: 1,
                        types: new[] { typeof(Expression) })
                    .MakeGenericMethod(resultType)
                    .Invoke(this, new[] { expression });

                return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
                    .MakeGenericMethod(resultType)
                    .Invoke(null, new[] { executionResult });
            }
        }

        private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable)
                : base(enumerable)
            { }

            public TestAsyncEnumerable(Expression expression)
                : base(expression)
            { }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }
        }

        private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public TestAsyncEnumerator(IEnumerator<T> inner)
            {
                _inner = inner;
            }

            public T Current => _inner.Current;

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(_inner.MoveNext());
            }

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return new ValueTask();
            }
        }
        
        #endregion
    }
}