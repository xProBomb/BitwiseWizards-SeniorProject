using Moq;
using TrustTrade.DAL.Abstract;
using TrustTrade.DAL.Concrete;
using TrustTrade.Models;
using Moq.EntityFrameworkCore;

namespace TestTrustTrade.Repositories;

[TestFixture]
public class CommentLikeRepositoryTests
{
    private Mock<TrustTradeDbContext> _mockDbContext;
    private ICommentLikeRepository _commentLikeRepository;
    private List<CommentLike> _commentLikes;

    [SetUp]
    public void Setup()
    {
        

        _commentLikes = new List<CommentLike>
        {
            new CommentLike
            {
                Id = 1,
                CommentId = 1,
                UserId = 1,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockDbContext = new Mock<TrustTradeDbContext>();
        _mockDbContext.Setup(c => c.CommentLikes).ReturnsDbSet(_commentLikes);
        _mockDbContext.Setup(c => c.Set<CommentLike>()).ReturnsDbSet(_commentLikes);

        _commentLikeRepository = new CommentLikeRepository(_mockDbContext.Object);
    }

    [Test]
    public async Task GetCommentLikesByCommentIdAsync_WhenLikesExist_ReturnsLikes()
    {
        // Arrange & Act
        var result = await _commentLikeRepository.FindByCommentIdAndUserIdAsync(1, 1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.CommentId, Is.EqualTo(1));
        Assert.That(result.UserId, Is.EqualTo(1));
    }

    [Test]
    public async Task GetCommentLikesByCommentIdAsync_WhenNoLikes_ReturnsNull()
    {
        // Arrange
        _commentLikes.Clear();

        // Act
        var result = await _commentLikeRepository.FindByCommentIdAndUserIdAsync(1, 1);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetLikeCountByCommentIdAsync_WhenLikesExist_ReturnsCount()
    {
        // Arrange & Act
        var result = await _commentLikeRepository.GetLikeCountByCommentIdAsync(1);

        // Assert
        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public async Task GetLikeCountByCommentIdAsync_WhenNoLikes_ReturnsZero()
    {
        // Arrange
        _commentLikes.Clear();

        // Act
        var result = await _commentLikeRepository.GetLikeCountByCommentIdAsync(1);

        // Assert
        Assert.That(result, Is.EqualTo(0));
    }
}