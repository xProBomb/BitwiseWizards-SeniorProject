using Moq;
using TrustTrade.DAL.Abstract;
using TrustTrade.Services.Web.Implementations;
using TrustTrade.Models;
using Microsoft.Extensions.Logging;
using TrustTrade.Services;

namespace TestTrustTrade;

[TestFixture]
public class CommentServiceTests
{
    private Mock<ILogger<CommentService>> _loggerMock;
    private Mock<ICommentRepository> _commentRepositoryMock;
    private CommentService _commentService;
    private Mock<IHoldingsRepository> _holdingsRepositoryMock;
    private Mock<IPostRepository> _postRepositoryMock;
    private Mock<ICommentLikeRepository> _commentLikeRepositoryMock;
    private Mock<INotificationService> _notificationServiceMock;
    private List<Comment> _comments;
    private List<Comment> _commentsWithPlaidEnabledUser;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<CommentService>>();
        _commentRepositoryMock = new Mock<ICommentRepository>();
        _holdingsRepositoryMock = new Mock<IHoldingsRepository>();
        _notificationServiceMock = new Mock<INotificationService>();
        _postRepositoryMock = new Mock<IPostRepository>();
        _commentLikeRepositoryMock = new Mock<ICommentLikeRepository>();
        

        _commentService = new CommentService(
            _loggerMock.Object,
            _commentRepositoryMock.Object,
            _holdingsRepositoryMock.Object,
            _notificationServiceMock.Object,
            _postRepositoryMock.Object,
            _commentLikeRepositoryMock.Object
        );

        var _user = new User
        {
            Id = 1,
            Username = "TestUser",
            PlaidEnabled = false,
        };

        var _plaidEnabledUser = new User
        {
            Id = 2,
            Username = "TestUser2",
            PlaidEnabled = true,
        };

        _comments = new List<Comment>
        {
            new Comment
            {
                Id = 1, 
                PostId = 1, 
                Content = "Comment 1",
                User = _user
            }
        };

        _commentsWithPlaidEnabledUser = new List<Comment>
        {
            new Comment
            {
                Id = 2, 
                PostId = 1, 
                Content = "Comment 2",
                User = _plaidEnabledUser,
                PortfolioValueAtPosting = 10000.00M
            }
        };


    }

    [Test]
    public async Task GetPostCommentsAsync_ValidPostId_ReturnsComments()
    {
        // Arrange
        _commentRepositoryMock.Setup(r => r.GetCommentsByPostIdAsync(1))
            .ReturnsAsync(_comments);

        // Act
        var result = await _commentService.GetPostCommentsAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(1));
        _commentRepositoryMock.Verify(r => r.GetCommentsByPostIdAsync(1), Times.Once);
    }

    [Test]
    public async Task GetPostCommentsAsync_ContainsPlaidEnabledUser_ReturnsCommentsWithPlaidInfo()
    {
        // Arrange
        _commentRepositoryMock.Setup(r => r.GetCommentsByPostIdAsync(1))
            .ReturnsAsync(_commentsWithPlaidEnabledUser);

        // Act
        var result = await _commentService.GetPostCommentsAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result[0].IsPlaidEnabled, Is.True);
        Assert.That(result[0].PortfolioValueAtPosting, Is.EqualTo("$10K"));
        _commentRepositoryMock.Verify(r => r.GetCommentsByPostIdAsync(1), Times.Once);
    }

    [Test]
    public async Task GetPostCommentsAsync_NoComments_ReturnsEmptyList()
    {
        // Arrange
        _commentRepositoryMock.Setup(r => r.GetCommentsByPostIdAsync(1))
            .ReturnsAsync(new List<Comment>());

        // Act
        var result = await _commentService.GetPostCommentsAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(0));
        _commentRepositoryMock.Verify(r => r.GetCommentsByPostIdAsync(1), Times.Once);
    }
}