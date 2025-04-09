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

        _posts = new List<Post>
        {
            new Post 
            { 
                Id = 1,
                UserId = user.Id,
                Title = "Post 1",
                Content = "Content 1",
                CreatedAt = DateTime.UtcNow,
                PortfolioValueAtPosting = 10000.00M,
                User = user,

            },
            new Post 
            { 
                Id = 2,
                UserId = user.Id,
                Title = "Post 2",
                Content = "Content 2",
                CreatedAt = DateTime.UtcNow,
                PortfolioValueAtPosting = 10000.00M,
                User = user,
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
        var result = await _postService.GetPostPreviewsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<List<PostPreviewVM>>());
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].Title, Is.EqualTo("Post 1"));
        Assert.That(result[1].Title, Is.EqualTo("Post 2"));
    }

    [Test]
    public async Task GetPostPreviewsAsync_WhenNoPostsExist_ReturnsEmptyList()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.GetPagedPostsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Post>());

        // Act
        var result = await _postService.GetPostPreviewsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<List<PostPreviewVM>>());
        Assert.That(result.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetFollowingPostPreviewsAsync_ReturnsPostPreviews()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.GetPagedPostsByUserFollowsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(_posts);

        // Act
        var result = await _postService.GetFollowingPostPreviewsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<List<PostPreviewVM>>());
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].Title, Is.EqualTo("Post 1"));
        Assert.That(result[1].Title, Is.EqualTo("Post 2"));
    }

    [Test]
    public async Task GetFollowingPostPreviewsAsync_WhenNoPostsExist_ReturnsEmptyList()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.GetPagedPostsByUserFollowsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Post>());

        // Act
        var result = await _postService.GetFollowingPostPreviewsAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<List<PostPreviewVM>>());
        Assert.That(result.Count, Is.EqualTo(0));
    }
}