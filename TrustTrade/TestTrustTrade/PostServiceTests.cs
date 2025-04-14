using Moq;
using TrustTrade.DAL.Abstract;
using TrustTrade.Services.Web.Implementations;
using TrustTrade.Models;
using Microsoft.Extensions.Logging;
using TrustTrade.ViewModels;

namespace TestTrustTrade;

[TestFixture]
public class PostServiceTests
{
    private Mock<ILogger<PostService>> _loggerMock;
    private Mock<IPostRepository> _postRepositoryMock;
    private Mock<ITagRepository> _tagRepositoryMock;
    private PostService _postService;

    private List<Post> _posts;
    private List<Post> _postsWithPlaidEnabledUser;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<PostService>>();
        _postRepositoryMock = new Mock<IPostRepository>();
        _tagRepositoryMock = new Mock<ITagRepository>();

        _postService = new PostService(
            _loggerMock.Object,
            _postRepositoryMock.Object,
            _tagRepositoryMock.Object
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
    public async Task GetPostPreviewsAsync_ReturnsPostPreviews()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.GetPagedPostsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(_posts);

        // Act
        var result = await _postService.GetPostPreviewsAsync("test-category", 1, "test-sort");

        // Assert
        Assert.That(result, Is.Not.Null.And.InstanceOf<List<PostPreviewVM>>());
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Title, Is.EqualTo("Post 1"));
        _postRepositoryMock.Verify(r => r.GetPagedPostsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task GetPostPreviewsAsync_WhenUserIsPlaidEnabled_ReturnsPostPreviewsWithPlaidStatus()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.GetPagedPostsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(_postsWithPlaidEnabledUser);

        // Act
        var result = await _postService.GetPostPreviewsAsync("test-category", 1, "test-sort");

        // Assert
        Assert.That(result, Is.Not.Null.And.InstanceOf<List<PostPreviewVM>>());
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Title, Is.EqualTo("Post 2"));
        Assert.That(result[0].IsPlaidEnabled, Is.True);
        Assert.That(result[0].PortfolioValueAtPosting, Is.EqualTo("$10K"));
        _postRepositoryMock.Verify(r => r.GetPagedPostsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task GetPostPreviewsAsync_WhenNoPostsExist_ReturnsEmptyList()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.GetPagedPostsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Post>());

        // Act
        var result = await _postService.GetPostPreviewsAsync("test-category", 1, "test-sort");

        // Assert
        Assert.That(result, Is.Not.Null.And.InstanceOf<List<PostPreviewVM>>());
        Assert.That(result.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetFollowingPostPreviewsAsync_ReturnsPostPreviews()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.GetPagedPostsByUserFollowsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(_posts);

        // Act
        var result = await _postService.GetFollowingPostPreviewsAsync(1, "test-category", 1, "test-sort");

        // Assert
        Assert.That(result, Is.Not.Null.And.InstanceOf<List<PostPreviewVM>>());
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Title, Is.EqualTo("Post 1"));
        _postRepositoryMock.Verify(r => r.GetPagedPostsByUserFollowsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task GetFollowingPostPreviewsAsync_WhenUserIsPlaidEnabled_ReturnsPostPreviewsWithPlaidStatus()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.GetPagedPostsByUserFollowsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(_postsWithPlaidEnabledUser);

        // Act
        var result = await _postService.GetFollowingPostPreviewsAsync(1, "test-category", 1, "test-sort");

        // Assert
        Assert.That(result, Is.Not.Null.And.InstanceOf<List<PostPreviewVM>>());
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Title, Is.EqualTo("Post 2"));
        Assert.That(result[0].IsPlaidEnabled, Is.True);
        Assert.That(result[0].PortfolioValueAtPosting, Is.EqualTo("$10K"));
        _postRepositoryMock.Verify(r => r.GetPagedPostsByUserFollowsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task GetFollowingPostPreviewsAsync_WhenNoPostsExist_ReturnsEmptyList()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.GetPagedPostsByUserFollowsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Post>());

        // Act
        var result = await _postService.GetFollowingPostPreviewsAsync(1, "test-category", 1, "test-sort");

        // Assert
        Assert.That(result, Is.Not.Null.And.InstanceOf<List<PostPreviewVM>>());
        Assert.That(result.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetUserPostPreviewsAsync_ReturnsPostPreviews()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.GetPagedPostsByUserAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(_posts);

        // Act
        var result = await _postService.GetUserPostPreviewsAsync(1, "test-category", 1, "test-sort");

        // Assert
        Assert.That(result, Is.Not.Null.And.InstanceOf<List<PostPreviewVM>>());
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Title, Is.EqualTo("Post 1"));
        _postRepositoryMock.Verify(r => r.GetPagedPostsByUserAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task GetUserPostPreviewsAsync_WhenUserIsPlaidEnabled_ReturnsPostPreviewsWithPlaidStatus()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.GetPagedPostsByUserAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(_postsWithPlaidEnabledUser);

        // Act
        var result = await _postService.GetUserPostPreviewsAsync(2, "test-category", 1, "test-sort");

        // Assert
        Assert.That(result, Is.Not.Null.And.InstanceOf<List<PostPreviewVM>>());
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Title, Is.EqualTo("Post 2"));
        Assert.That(result[0].IsPlaidEnabled, Is.True);
        Assert.That(result[0].PortfolioValueAtPosting, Is.EqualTo("$10K"));
        _postRepositoryMock.Verify(r => r.GetPagedPostsByUserAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task GetUserPostsPreviewsAsync_WhenNoPostsExist_ReturnsEmptyList()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.GetPagedPostsByUserAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Post>());

        // Act
        var result = await _postService.GetUserPostPreviewsAsync(1, "test-category", 1, "test-sort");

        // Assert
        Assert.That(result, Is.Not.Null.And.InstanceOf<List<PostPreviewVM>>());
        Assert.That(result.Count, Is.EqualTo(0));
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
        _postRepositoryMock.Setup(r => r.GetTotalPostsAsync(It.IsAny<string>()))
            .ReturnsAsync(25);

        // Act
        var result = await _postService.BuildPaginationAsync(null, 2);

        // Assert
        Assert.That(result, Is.Not.Null.And.InstanceOf<PaginationPartialVM>());
        Assert.That(result.CurrentPage, Is.EqualTo(2));
        Assert.That(result.TotalPages, Is.EqualTo(3)); // 25 posts / 10 per page = 3 pages
        Assert.That(result.PagesToShow, Is.EquivalentTo(new List<int> { 1, 2, 3 }));
        Assert.That(result.CategoryFilter, Is.Null);
        _postRepositoryMock.Verify(repo => repo.GetTotalPostsAsync(It.IsAny<string>()), Times.Once);
    }
}