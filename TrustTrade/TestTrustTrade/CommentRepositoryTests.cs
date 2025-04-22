using Moq;
using TrustTrade.DAL.Abstract;
using TrustTrade.DAL.Concrete;
using TrustTrade.Models;
using Moq.EntityFrameworkCore;

namespace TestTrustTrade;

[TestFixture]
public class CommentRepositoryTests
{
    private Mock<TrustTradeDbContext> _mockDbContext;
    private List<Comment> _comments;
    private ICommentRepository _commentRepository;

    [SetUp]
    public void Setup()
    {
        

        _comments = new List<Comment>
        {
            new Comment
                {
                    Id = 1,
                    PostId = 1,
                    UserId = 1,
                    Content = "Test comment",
                    CreatedAt = DateTime.UtcNow
                }
        };

        _mockDbContext = new Mock<TrustTradeDbContext>();
        _mockDbContext.Setup(c => c.Comments).ReturnsDbSet(_comments);
        _mockDbContext.Setup(c => c.Set<Comment>()).ReturnsDbSet(_comments);

        _commentRepository = new CommentRepository(_mockDbContext.Object);
    }

    [Test]
    public async Task GetCommentsByPostIdAsync_WhenCommentsExist_ReturnsComments()
    {
        // Arrange & Act
        var result = await _commentRepository.GetCommentsByPostIdAsync(1);

        // Assert
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(1));
    }

    [Test]
    public async Task GetCommentsByPostIdAsync_WhenNoComments_ReturnsEmptyList()
    {
        // Arrange
        _comments.Clear();

        // Act
        var result = await _commentRepository.GetCommentsByPostIdAsync(1);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetCommentsByPostIdAsync_WhenPostDoesNotExist_ReturnsEmptyList()
    {
        // Arrange & Act
        var result = await _commentRepository.GetCommentsByPostIdAsync(999);

        // Assert
        Assert.That(result, Is.Empty);
    }
}