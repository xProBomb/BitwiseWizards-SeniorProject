using Moq;
using TrustTrade.DAL.Abstract;
using TrustTrade.Services.Web.Implementations;
using TrustTrade.Models;
using Microsoft.Extensions.Logging;
using TrustTrade.ViewModels;
using TrustTrade.DAL.Concrete;
using Microsoft.IdentityModel.Logging;

namespace TestTrustTrade;

[TestFixture]
public class PostServiceTests
{
    private Mock<ILogger<PostService>> _loggerMock;
    private Mock<IPostRepository> _postRepositoryMock;
    private Mock<ITagRepository> _tagRepositoryMock;
    private Mock<IUserBlockRepository> _userBlockRepositoryMock;
    private Mock<ISavedPostRepository> _savedPostRepositoryMock;
    private PostService _postService;
    private List<Post> _posts;
    private List<Post> _postsWithPlaidEnabledUser;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<PostService>>();
        _postRepositoryMock = new Mock<IPostRepository>();
        _tagRepositoryMock = new Mock<ITagRepository>();
        _userBlockRepositoryMock = new Mock<IUserBlockRepository>();
        _savedPostRepositoryMock = new Mock<ISavedPostRepository>();

        _postService = new PostService(
            _loggerMock.Object,
            _postRepositoryMock.Object,
            _tagRepositoryMock.Object,
            _userBlockRepositoryMock.Object,
            _savedPostRepositoryMock.Object
        );

        var user = new User
        {
            Id = 1,
            IdentityId = "test-identity-1",
            ProfileName = "johnDoe",
            Username = "johnDoe",
            Email = "johnDoe@example.com",
            PlaidEnabled = false
        };

        var plaidEnabledUser = new User
        {
            Id = 2,
            IdentityId = "test-identity-2",
            ProfileName = "janeSmith",
            Username = "janeSmith",
            Email = "janeSmith@example.com",
            PlaidEnabled = true
        };

        _posts = new List<Post>
        {
            new Post 
            { 
                Id = 1,
                UserId = user.Id,
                Title = "Post 1",
                Content = "Content 1",
                CreatedAt = DateTime.UtcNow,
                User = user,

            }
        };

        _postsWithPlaidEnabledUser = new List<Post>
        {
            new Post 
            { 
                Id = 2,
                UserId = plaidEnabledUser.Id,
                Title = "Post 2",
                Content = "Content 2",
                CreatedAt = DateTime.UtcNow,
                PortfolioValueAtPosting = 10000.00M,
                User = plaidEnabledUser,
            }
        };
    }

    [Test]
    public async Task GetPagedPostsAsync_ReturnsPosts()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.GetPagedPostsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<int>>()))
            .ReturnsAsync((_posts, _posts.Count));

        // Act
        var (posts, totalPosts) = await _postService.GetPagedPostsAsync("test-category", 1, "test-sort", null);

        // Assert
        Assert.That(posts, Is.Not.Null.And.InstanceOf<List<Post>>());
        Assert.That(posts.Count, Is.EqualTo(1));
        Assert.That(posts[0].Title, Is.EqualTo("Post 1"));
        _postRepositoryMock.Verify(r => r.GetPagedPostsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<int>>()), Times.Once);
    }

    [Test]
    public async Task GetPagedPostsAsync_WhenNoPostsExist_ReturnsEmptyList()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.GetPagedPostsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<int>>()))
            .ReturnsAsync((new List<Post>(), 0));

        // Act
        var (posts, totalPosts) = await _postService.GetPagedPostsAsync("test-category", 1, "test-sort", null);

        // Assert
        Assert.That(posts, Is.Not.Null.And.InstanceOf<List<Post>>());
        Assert.That(posts.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetFollowingPagedPostsAsync_ReturnsPosts()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.GetPagedPostsByUserFollowsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<int>>()))
            .ReturnsAsync((_posts, _posts.Count));

        // Act
        var (posts, totalPosts) = await _postService.GetFollowingPagedPostsAsync(1, "test-category", 1, "test-sort");

        // Assert
        Assert.That(posts, Is.Not.Null.And.InstanceOf<List<Post>>());
        Assert.That(posts.Count, Is.EqualTo(1));
        Assert.That(posts[0].Title, Is.EqualTo("Post 1"));
        _postRepositoryMock.Verify(r => r.GetPagedPostsByUserFollowsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<int>>()), Times.Once);
    }

    [Test]
    public async Task GetFollowingPagedPostsAsync_WhenNoPostsExist_ReturnsEmptyList()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.GetPagedPostsByUserFollowsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<int>>()))
            .ReturnsAsync((new List<Post>(), 0));

        // Act
        var (posts, totalPosts) = await _postService.GetFollowingPagedPostsAsync(1, "test-category", 1, "test-sort");

        // Assert
        Assert.That(posts, Is.Not.Null.And.InstanceOf<List<Post>>());
        Assert.That(posts.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetUserPagedPostsAsync_ReturnsPosts()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.GetPagedPostsByUserAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<int>>()))
            .ReturnsAsync((_posts, _posts.Count));

        // Act
        var (posts, totalPosts) = await _postService.GetUserPagedPostsAsync(1, "test-category", 1, "test-sort");

        // Assert
        Assert.That(posts, Is.Not.Null.And.InstanceOf<List<Post>>());
        Assert.That(posts.Count, Is.EqualTo(1));
        Assert.That(posts[0].Title, Is.EqualTo("Post 1"));
        _postRepositoryMock.Verify(r => r.GetPagedPostsByUserAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<int>>()), Times.Once);
    }

    [Test]
    public async Task GetUserPagedPostsAsync_WhenNoPostsExist_ReturnsEmptyList()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.GetPagedPostsByUserAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<int>>()))
            .ReturnsAsync((new List<Post>(), 0));

        // Act
        var (posts, totalPosts) = await _postService.GetUserPagedPostsAsync(1, "test-category", 1, "test-sort");

        // Assert
        Assert.That(posts, Is.Not.Null.And.InstanceOf<List<Post>>());
        Assert.That(posts.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task BuildPostFiltersAsync_ReturnsCorrectPostFilters()
    {
        // Arrange
        var tags = new List<string> { "tag1", "tag2", "tag3" };
        _tagRepositoryMock.Setup(r => r.GetAllTagNamesAsync())
            .ReturnsAsync(tags);

        // Act
        var result = await _postService.BuildPostFiltersAsync("tag1", "DateDesc");

        // Assert
        Assert.That(result, Is.Not.Null.And.InstanceOf<PostFiltersPartialVM>());
        Assert.That(result.SelectedCategory, Is.EqualTo("tag1"));
        Assert.That(result.SortOrder, Is.EqualTo("DateDesc"));
        Assert.That(result.Categories, Is.EquivalentTo(tags));
        _tagRepositoryMock.Verify(r => r.GetAllTagNamesAsync(), Times.Once);
    }

    [Test]
    public async Task BuildPostFiltersAsync_WhenNoTagsExist_ReturnsEmptyTags()
    {
        // Arrange
        _tagRepositoryMock.Setup(r => r.GetAllTagNamesAsync())
            .ReturnsAsync(new List<string>());

        // Act
        var result = await _postService.BuildPostFiltersAsync("tag1", "DateDesc");

        // Assert
        Assert.That(result, Is.Not.Null.And.InstanceOf<PostFiltersPartialVM>());
        Assert.That(result.SelectedCategory, Is.EqualTo("tag1"));
        Assert.That(result.SortOrder, Is.EqualTo("DateDesc"));
        Assert.That(result.Categories, Is.Empty);
        _tagRepositoryMock.Verify(r => r.GetAllTagNamesAsync(), Times.Once);
    }

    [Test]
    public async Task BuildPaginationAsync_ReturnsCorrectPagination()
    {
        // Arrange

        // Act
        var result = await _postService.BuildPaginationAsync(null, 2, 25, null);

        // Assert
        Assert.That(result, Is.Not.Null.And.InstanceOf<PaginationPartialVM>());
        Assert.That(result.CurrentPage, Is.EqualTo(2));
        Assert.That(result.TotalPages, Is.EqualTo(3)); // 25 posts / 10 per page = 3 pages
        Assert.That(result.PagesToShow, Is.EquivalentTo(new List<int> { 1, 2, 3 }));
        Assert.That(result.CategoryFilter, Is.Null);
    }
}