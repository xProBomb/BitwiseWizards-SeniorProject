using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System.Security.Claims;
using TrustTrade.Controllers;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using TrustTrade.ViewModels;

namespace TestTrustTrade
{
    [TestFixture]
    public class ProfileControllerTests
    {
        private Mock<TrustTradeDbContext> _contextMock;
        private Mock<IHoldingsRepository> _holdingsRepoMock;
        private Mock<ILogger<ProfileController>> _loggerMock;
        private Mock<IProfileService> _profileServiceMock;
        private Mock<IPerformanceScoreRepository> _performanceScoreRepository;
        private ProfileController _controller;
        
        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [SetUp]
        public void Setup()
        {
            _contextMock = new Mock<TrustTradeDbContext>();
            _holdingsRepoMock = new Mock<IHoldingsRepository>();
            _loggerMock = new Mock<ILogger<ProfileController>>();
            _profileServiceMock = new Mock<IProfileService>();
            _performanceScoreRepository = new Mock<IPerformanceScoreRepository>();

            _controller = new ProfileController(
                _contextMock.Object,
                _holdingsRepoMock.Object,
                _loggerMock.Object,
                _profileServiceMock.Object,
                _performanceScoreRepository.Object
            );
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

        private void SetupAuthentication(string identityId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, identityId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };
        }
        
        [Test]
        public async Task MyProfile_NoIdentityClaim_ReturnsUnauthorized()
        {
            // Arrange
            SetupAuthentication(""); // Empty identity claim

            // Act
            var result = await _controller.MyProfile();

            // Assert
            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
        }

        [Test]
        public async Task MyProfile_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var testIdentityId = "nonexistent_identity";
            var emptyUserList = new List<User>().AsQueryable();
            var mockDbSet = CreateMockDbSet(emptyUserList);
            _contextMock.Setup(c => c.Users).Returns(mockDbSet.Object);

            SetupAuthentication(testIdentityId);

            // Act
            var result = await _controller.MyProfile();

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        // Helper classes for async operations
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
    }
}