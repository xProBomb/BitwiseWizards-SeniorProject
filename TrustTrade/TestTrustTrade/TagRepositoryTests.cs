using Moq;
using TrustTrade.DAL.Abstract;
using TrustTrade.DAL.Concrete;
using TrustTrade.Models;
using Microsoft.EntityFrameworkCore;

namespace TestTrustTrade;

[TestFixture]
public class TagRepositoryTests
{
    private Mock<TrustTradeDbContext> _mockDbContext;
    private Mock<DbSet<Tag>> _mockTagDbSet;
    private List<Tag> _tags;

    // A helper to make dbset queryable
    private Mock<DbSet<T>> GetMockDbSet<T>(IQueryable<T> entities) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(entities.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(entities.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(entities.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(entities.GetEnumerator());
        return mockSet;
    }


    [SetUp]
    public void Setup()
    {
        

        _tags = new List<Tag>
        {
            new Tag { Id = 1, TagName = "Memes" },
            new Tag { Id = 2, TagName = "Gain" },
            new Tag { Id = 3, TagName = "Loss" },
            new Tag { Id = 4, TagName = "Stocks" },
            new Tag { Id = 5, TagName = "Crypto" }
        };

        _mockDbContext = new Mock<TrustTradeDbContext>();
        _mockTagDbSet = GetMockDbSet(_tags.AsQueryable());
        _mockDbContext.Setup(c => c.Tags).Returns(_mockTagDbSet.Object);
        _mockDbContext.Setup(c => c.Set<Tag>()).Returns(_mockTagDbSet.Object); 
    }

    [Test]
    public void GetAllTagNames_WhenCalled_ReturnsAllTagNames()
    {
        // Arrange
        ITagRepository tagRepository = new TagRepository(_mockDbContext.Object);
        var expected = new List<string> { "Memes", "Gain", "Loss", "Stocks", "Crypto" };

        // Act
        var result = tagRepository.GetAllTagNames();

        // Assert
        Assert.That(result, Is.EquivalentTo(expected));
    }
}