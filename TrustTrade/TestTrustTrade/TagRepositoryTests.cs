using Moq;
using TrustTrade.DAL.Abstract;
using TrustTrade.DAL.Concrete;
using TrustTrade.Models;
using Microsoft.EntityFrameworkCore;
using Moq.EntityFrameworkCore;

namespace TestTrustTrade;

[TestFixture]
public class TagRepositoryTests
{
    private Mock<TrustTradeDbContext> _mockDbContext;
    private List<Tag> _tags;

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
        _mockDbContext.Setup(c => c.Tags).ReturnsDbSet(_tags);
        _mockDbContext.Setup(c => c.Set<Tag>()).ReturnsDbSet(_tags);
    }

    [Test]
    public async Task GetAllTagNamesAsync_WhenTagsExist_ReturnsAllTagNames()
    {
        // Arrange
        ITagRepository tagRepository = new TagRepository(_mockDbContext.Object);
        var expected = new List<string> { "Memes", "Gain", "Loss", "Stocks", "Crypto" };

        // Act
        var result = await tagRepository.GetAllTagNamesAsync();

        // Assert
        Assert.That(result, Is.EquivalentTo(expected));
    }

    [Test]
    public async Task GetAllTagNamesAsync_WhenNoTags_ReturnsEmptyList()
    {
        // Arrange
        _tags.Clear();
        ITagRepository tagRepository = new TagRepository(_mockDbContext.Object);

        // Act
        var result = await tagRepository.GetAllTagNamesAsync();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [TestCase("Memes")]
    [TestCase("memes")] // Case insensitivity test
    [TestCase("MEMES")] // Case insensitivity test
    public async Task FindByTagNameAsync_WhenTagExists_ReturnsTag(string tagName)
    {
        // Arrange
        ITagRepository tagRepository = new TagRepository(_mockDbContext.Object);
        var expected = _tags[0];

        // Act
        var result = await tagRepository.FindByTagNameAsync(tagName);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(expected));
        });
    }

    [Test]
    public async Task FindByTagNameAsync_WhenTagDoesNotExist_ReturnsNull()
    {
        // Arrange
        ITagRepository tagRepository = new TagRepository(_mockDbContext.Object);

        // Act
        var result = await tagRepository.FindByTagNameAsync("NonExistentTag");

        // Assert
        Assert.That(result, Is.Null);
    }
}