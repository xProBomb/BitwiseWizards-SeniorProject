using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using TrustTrade.Controllers;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using TrustTrade.Services;
using TrustTrade.ViewModels;
using TrustTrade.Services.Web.Interfaces;

namespace TestTrustTrade
{
    /// <summary>
    /// Test suite specifically for profile editing functionality - focused on Bio and Trading Preference fields.
    /// These tests validate the behavior of ProfileController.UpdateProfile method for bio and userTag updates.
    /// </summary>
    [TestFixture]
    public class ProfileEditingTests
    {
        private Mock<TrustTradeDbContext> _contextMock;
        private Mock<IHoldingsRepository> _holdingsRepoMock;
        private Mock<ILogger<ProfileController>> _loggerMock;
        private Mock<IPostService> _postServiceMock;
        private Mock<IProfileService> _profileServiceMock;
        private Mock<IUserService> _userServiceMock;
        private Mock<IPerformanceScoreRepository> _performanceScoreRepository;
        private ProfileController _controller;
        private User _user;
        private List<PostPreviewVM> _postPreviews;
        private PostFiltersPartialVM _postFiltersPartialVM;
        private PaginationPartialVM _paginationPartialVM;
        private Mock<INotificationService> _notificationServiceMock;
        
        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [SetUp]
        public void Setup()
        {
            // Initialize all required mocks for the ProfileController
            _contextMock = new Mock<TrustTradeDbContext>();
            _holdingsRepoMock = new Mock<IHoldingsRepository>();
            _loggerMock = new Mock<ILogger<ProfileController>>();
            _postServiceMock = new Mock<IPostService>();
            _profileServiceMock = new Mock<IProfileService>();
            _userServiceMock = new Mock<IUserService>();
            _performanceScoreRepository = new Mock<IPerformanceScoreRepository>();
            _notificationServiceMock = new Mock<INotificationService>();

            _controller = new ProfileController(
                _contextMock.Object,
                _holdingsRepoMock.Object,
                _loggerMock.Object,
                _postServiceMock.Object,
                _profileServiceMock.Object,
                _userServiceMock.Object,
                _performanceScoreRepository.Object,
                _notificationServiceMock.Object
            );

            // Mock users
            _user = new User
            {
                Id = 1,
                IdentityId = "test-identity-1",
                ProfileName = "johnDoe",
                Username = "johnDoe",
                Email = "johndoe@example.com",
                PasswordHash = "dummyHash"
            };

            _postPreviews = new List<PostPreviewVM>
            {
                new PostPreviewVM
                    {
                        Id = 1,
                        Title = "Post 1",
                        IsPlaidEnabled = true
                    }
            };

            _postFiltersPartialVM = new PostFiltersPartialVM
            {
                SelectedCategory = "Category1",
            };

            _paginationPartialVM = new PaginationPartialVM
            {
                CurrentPage = 1,
                TotalPages = 3
            };
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
        
        /// <summary>
        /// Tests that the Bio field is properly updated when a valid bio is provided.
        /// </summary>
        [Test]
        public async Task UpdateProfile_ValidBio_UpdatesBio()
        {
            // Arrange
            SetupAuthentication("testUserId");
            var initialBio = "This is my initial bio";
            var newBio = "This is my updated bio with new information.";
            
            var currentUser = new User
            {
                IdentityId = "testUserId",
                Username = "testUser",
                Bio = initialBio,
                UserTag = "Stocks",
                CreatedAt = DateTime.Now,
                FollowerFollowerUsers = new List<Follower>(),
                FollowerFollowingUsers = new List<Follower>()
            };
            
            var users = new List<User> { currentUser }.AsQueryable();
            var mockSet = CreateMockDbSet(users);
            _contextMock.Setup(c => c.Users).Returns(mockSet.Object);
            _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            
            // Act - only changing the bio
            var result = await _controller.UpdateProfile("testUser", newBio, "Stocks");
            
            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>(), "Result should be a redirect");
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult.ActionName, Is.EqualTo("MyProfile"), "Should redirect to MyProfile");
            
            // Verify the bio was updated but other fields remained the same
            Assert.That(currentUser.Bio, Is.EqualTo(newBio), "Bio should be updated to the new value");
            Assert.That(currentUser.Username, Is.EqualTo("testUser"), "Username should remain unchanged");
            Assert.That(currentUser.UserTag, Is.EqualTo("Stocks"), "UserTag should remain unchanged");
        }
        
        /// <summary>
        /// Tests that the Bio field can be set to an empty string.
        /// </summary>
        [Test]
        public async Task UpdateProfile_EmptyBio_UpdatesWithEmptyBio()
        {
            // Arrange
            SetupAuthentication("testUserId");
            var initialBio = "This is my initial bio";
            var emptyBio = "";
            
            var currentUser = new User
            {
                IdentityId = "testUserId",
                Username = "testUser",
                Bio = initialBio,
                UserTag = "Stocks",
                CreatedAt = DateTime.Now,
                FollowerFollowerUsers = new List<Follower>(),
                FollowerFollowingUsers = new List<Follower>()
            };
            
            var users = new List<User> { currentUser }.AsQueryable();
            var mockSet = CreateMockDbSet(users);
            _contextMock.Setup(c => c.Users).Returns(mockSet.Object);
            _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            
            // Act - setting bio to empty string
            var result = await _controller.UpdateProfile("testUser", emptyBio, "Stocks");
            
            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>(), "Result should be a redirect");
            
            // Verify bio was set to empty string
            Assert.That(currentUser.Bio, Is.EqualTo(emptyBio), "Bio should be updated to empty string");
        }
        
        /// <summary>
        /// Tests that the Bio field can be set to null.
        /// </summary>
        [Test]
        public async Task UpdateProfile_NullBio_UpdatesWithNullBio()
        {
            // Arrange
            SetupAuthentication("testUserId");
            var initialBio = "This is my initial bio";
            string nullBio = null;
            
            var currentUser = new User
            {
                IdentityId = "testUserId",
                Username = "testUser",
                Bio = initialBio,
                UserTag = "Stocks",
                CreatedAt = DateTime.Now,
                FollowerFollowerUsers = new List<Follower>(),
                FollowerFollowingUsers = new List<Follower>()
            };
            
            var users = new List<User> { currentUser }.AsQueryable();
            var mockSet = CreateMockDbSet(users);
            _contextMock.Setup(c => c.Users).Returns(mockSet.Object);
            _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            
            // Act - setting bio to null
            var result = await _controller.UpdateProfile("testUser", nullBio, "Stocks");
            
            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>(), "Result should be a redirect");
            
            // Verify bio was set to null
            Assert.That(currentUser.Bio, Is.Null, "Bio should be updated to null");
        }
        
        /// <summary>
        /// Tests that a long bio can be properly saved.
        /// </summary>
        [Test]
        public async Task UpdateProfile_LongBio_UpdatesWithLongBio()
        {
            // Arrange
            SetupAuthentication("testUserId");
            var initialBio = "Short bio";
            var longBio = new string('A', 500); // 500 character bio (the model allows up to 500 characters)
            
            var currentUser = new User
            {
                IdentityId = "testUserId",
                Username = "testUser",
                Bio = initialBio,
                UserTag = "Stocks",
                CreatedAt = DateTime.Now,
                FollowerFollowerUsers = new List<Follower>(),
                FollowerFollowingUsers = new List<Follower>()
            };
            
            var users = new List<User> { currentUser }.AsQueryable();
            var mockSet = CreateMockDbSet(users);
            _contextMock.Setup(c => c.Users).Returns(mockSet.Object);
            _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            
            // Act - setting long bio
            var result = await _controller.UpdateProfile("testUser", longBio, "Stocks");
            
            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>(), "Result should be a redirect");
            
            // Verify long bio was saved correctly
            Assert.That(currentUser.Bio, Is.EqualTo(longBio), "Long bio should be saved correctly");
            Assert.That(currentUser.Bio.Length, Is.EqualTo(500), "Bio length should be 500 characters");
        }
        
        /// <summary>
        /// Tests that the UserTag field is properly updated when a valid tag is provided.
        /// </summary>
        [Test]
        public async Task UpdateProfile_ValidUserTag_UpdatesUserTag()
        {
            // Arrange
            SetupAuthentication("testUserId");
            var initialUserTag = "Stocks";
            var newUserTag = "Options";
            
            var currentUser = new User
            {
                IdentityId = "testUserId",
                Username = "testUser",
                Bio = "Test bio",
                UserTag = initialUserTag,
                CreatedAt = DateTime.Now,
                FollowerFollowerUsers = new List<Follower>(),
                FollowerFollowingUsers = new List<Follower>()
            };
            
            var users = new List<User> { currentUser }.AsQueryable();
            var mockSet = CreateMockDbSet(users);
            _contextMock.Setup(c => c.Users).Returns(mockSet.Object);
            _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            
            // Act - only changing the userTag
            var result = await _controller.UpdateProfile("testUser", "Test bio", newUserTag);
            
            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>(), "Result should be a redirect");
            
            // Verify userTag was updated but other fields remained the same
            Assert.That(currentUser.UserTag, Is.EqualTo(newUserTag), "UserTag should be updated to the new value");
            Assert.That(currentUser.Username, Is.EqualTo("testUser"), "Username should remain unchanged");
            Assert.That(currentUser.Bio, Is.EqualTo("Test bio"), "Bio should remain unchanged");
        }
        
        /// <summary>
        /// Tests that the UserTag field can be set to null.
        /// </summary>
        [Test]
        public async Task UpdateProfile_NullUserTag_UpdatesWithNullUserTag()
        {
            // Arrange
            SetupAuthentication("testUserId");
            var initialUserTag = "Stocks";
            string nullUserTag = null;
            
            var currentUser = new User
            {
                IdentityId = "testUserId",
                Username = "testUser",
                Bio = "Test bio",
                UserTag = initialUserTag,
                CreatedAt = DateTime.Now,
                FollowerFollowerUsers = new List<Follower>(),
                FollowerFollowingUsers = new List<Follower>()
            };
            
            var users = new List<User> { currentUser }.AsQueryable();
            var mockSet = CreateMockDbSet(users);
            _contextMock.Setup(c => c.Users).Returns(mockSet.Object);
            _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            
            // Act - setting userTag to null
            var result = await _controller.UpdateProfile("testUser", "Test bio", nullUserTag);
            
            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>(), "Result should be a redirect");
            
            // Verify userTag was set to null
            Assert.That(currentUser.UserTag, Is.Null, "UserTag should be updated to null");
        }
        
        /// <summary>
        /// Tests that the UserTag field can be set to an empty string.
        /// </summary>
        [Test]
        public async Task UpdateProfile_EmptyUserTag_UpdatesWithEmptyUserTag()
        {
            // Arrange
            SetupAuthentication("testUserId");
            var initialUserTag = "Stocks";
            var emptyUserTag = "";
            
            var currentUser = new User
            {
                IdentityId = "testUserId",
                Username = "testUser",
                Bio = "Test bio",
                UserTag = initialUserTag,
                CreatedAt = DateTime.Now,
                FollowerFollowerUsers = new List<Follower>(),
                FollowerFollowingUsers = new List<Follower>()
            };
            
            var users = new List<User> { currentUser }.AsQueryable();
            var mockSet = CreateMockDbSet(users);
            _contextMock.Setup(c => c.Users).Returns(mockSet.Object);
            _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            
            // Act - setting userTag to empty string
            var result = await _controller.UpdateProfile("testUser", "Test bio", emptyUserTag);
            
            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>(), "Result should be a redirect");
            
            // Verify userTag was set to empty string
            Assert.That(currentUser.UserTag, Is.EqualTo(emptyUserTag), "UserTag should be updated to empty string");
        }
        
        /// <summary>
        /// Tests that all profile fields (Username, Bio, and UserTag) can be updated simultaneously.
        /// </summary>
        [Test]
        public async Task UpdateProfile_AllFields_UpdatesAllFields()
        {
            // Arrange
            SetupAuthentication("testUserId");
            var currentUser = new User
            {
                IdentityId = "testUserId",
                Username = "oldUsername",
                Bio = "Old bio",
                UserTag = "Stocks",
                CreatedAt = DateTime.Now,
                FollowerFollowerUsers = new List<Follower>(),
                FollowerFollowingUsers = new List<Follower>()
            };
            
            var users = new List<User> { currentUser }.AsQueryable();
            var mockSet = CreateMockDbSet(users);
            _contextMock.Setup(c => c.Users).Returns(mockSet.Object);
            _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            
            // Act - update all fields
            var result = await _controller.UpdateProfile("newUsername", "New bio content", "Crypto");
            
            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>(), "Result should be a redirect");
            
            // Verify all fields were updated
            Assert.That(currentUser.Username, Is.EqualTo("newUsername"), "Username should be updated");
            Assert.That(currentUser.Bio, Is.EqualTo("New bio content"), "Bio should be updated");
            Assert.That(currentUser.UserTag, Is.EqualTo("Crypto"), "UserTag should be updated");
        }
        
        /// <summary>
        /// Tests that user not found returns a NotFound result.
        /// </summary>
        [Test]
        public async Task UpdateProfile_UserNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            SetupAuthentication("nonExistentUserId");
            var emptyUserList = new List<User>().AsQueryable();
            var mockSet = CreateMockDbSet(emptyUserList);
            _contextMock.Setup(c => c.Users).Returns(mockSet.Object);
            
            // Act
            var result = await _controller.UpdateProfile("username", "Bio", "Options");
            
            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>(), "Should return NotFound when user doesn't exist");
        }

        [Test]
        public async Task UserPosts_ReturnsAViewResult()
        {
            // Arrange
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(It.IsAny<string>()))
                .ReturnsAsync(_user);
            _postServiceMock.Setup(s => s.GetUserPostPreviewsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(_postPreviews);
            _postServiceMock.Setup(s => s.BuildPostFiltersAsync(It.IsAny<string>(), It.IsAny<string>(), null))
                .ReturnsAsync(_postFiltersPartialVM);
            _postServiceMock.Setup(s => s.BuildUserPaginationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(_paginationPartialVM);
            
            // Act
            var result = await _controller.UserPosts(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>());

            Assert.That(result, Is.Not.Null.And.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task UserPosts_ReturnsUserPostsVM()
        {
            // Arrange
            _userServiceMock.Setup(s => s.GetUserByUsernameAsync(It.IsAny<string>()))
                .ReturnsAsync(_user);
            _postServiceMock.Setup(s => s.GetUserPostPreviewsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(_postPreviews);
            _postServiceMock.Setup(s => s.BuildPostFiltersAsync(It.IsAny<string>(), It.IsAny<string>(), null))
                .ReturnsAsync(_postFiltersPartialVM);
            _postServiceMock.Setup(s => s.BuildUserPaginationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(_paginationPartialVM);

            // Act
            var result = await _controller.UserPosts(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()) as ViewResult;
            var model = result?.Model as UserPostsVM;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(model, Is.Not.Null.And.InstanceOf<UserPostsVM>());
            Assert.That(model.Posts, Is.EqualTo(_postPreviews));
            Assert.That(model.PostFilters, Is.EqualTo(_postFiltersPartialVM));
            Assert.That(model.Pagination, Is.EqualTo(_paginationPartialVM));
        }

        /// <summary>
        /// Helper classes for async operations
        /// </summary>
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