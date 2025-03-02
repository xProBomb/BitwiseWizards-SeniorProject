using Moq;
using TrustTrade.DAL.Abstract;
using TrustTrade.DAL.Concrete;
using TrustTrade.Models;
using Microsoft.EntityFrameworkCore;

namespace TestTrustTrade;

[TestFixture]
public class PostRepositoryTests
{
    private Mock<TrustTradeDbContext> _mockDbContext;
    private Mock<DbSet<Post>> _mockPostDbSet;
    private List<Post> _posts;
    private List<User> _users;
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
        _posts = new List<Post>
        {
            new Post { Id = 1, UserId = 1, Title = "Post 1", Content = "Content 1", CreatedAt = DateTime.Now.AddMinutes(-1) },
            new Post { Id = 2, UserId = 2, Title = "Post 2", Content = "Content 2", CreatedAt = DateTime.Now.AddHours(-1) },
            new Post { Id = 3, UserId = 3, Title = "Post 3", Content = "Content 3", CreatedAt = DateTime.Now.AddDays(-1) },
            new Post { Id = 4, UserId = 4, Title = "Post 4", Content = "Content 4", CreatedAt = DateTime.Now.AddMonths(-1) },
            new Post { Id = 5, UserId = 5, Title = "Post 5", Content = "Content 5", CreatedAt = DateTime.Now.AddYears(-1) },
            new Post { Id = 6, UserId = 6, Title = "Post 6", Content = "Content 6", CreatedAt = DateTime.Now.AddMinutes(-3) },
            new Post { Id = 7, UserId = 7, Title = "Post 7", Content = "Content 7", CreatedAt = DateTime.Now.AddHours(-3) },
            new Post { Id = 8, UserId = 8, Title = "Post 8", Content = "Content 8", CreatedAt = DateTime.Now.AddDays(-3) },
            new Post { Id = 9, UserId = 9, Title = "Post 9", Content = "Content 9", CreatedAt = DateTime.Now.AddMonths(-3) },
            new Post { Id = 10, UserId = 10, Title = "Post 10", Content = "Content 10", CreatedAt = DateTime.Now.AddYears(-3) },
            new Post { Id = 11, UserId = 11, Title = "Post 11", Content = "Content 11", CreatedAt = DateTime.Now.AddMinutes(-2) },
            new Post { Id = 12, UserId = 12, Title = "Post 12", Content = "Content 12", CreatedAt = DateTime.Now.AddHours(-2) },
            new Post { Id = 13, UserId = 13, Title = "Post 13", Content = "Content 13", CreatedAt = DateTime.Now.AddDays(-2) },
            new Post { Id = 14, UserId = 14, Title = "Post 14", Content = "Content 14", CreatedAt = DateTime.Now.AddMonths(-2) },
            new Post { Id = 15, UserId = 15, Title = "Post 15", Content = "Content 15", CreatedAt = DateTime.Now.AddYears(-2) },
            new Post { Id = 16, UserId = 16, Title = "Post 16", Content = "Content 16", CreatedAt = DateTime.Now.AddMinutes(-4) },
            new Post { Id = 17, UserId = 17, Title = "Post 17", Content = "Content 17", CreatedAt = DateTime.Now.AddHours(-4) },
            new Post { Id = 18, UserId = 18, Title = "Post 18", Content = "Content 18", CreatedAt = DateTime.Now.AddDays(-4) },
            new Post { Id = 19, UserId = 19, Title = "Post 19", Content = "Content 19", CreatedAt = DateTime.Now.AddMonths(-4) },
            new Post { Id = 20, UserId = 20, Title = "Post 20", Content = "Content 20", CreatedAt = DateTime.Now.AddYears(-4) }
        };

        _users = new List<User>
        {
            new User { Id = 1, ProfileName = "User 1", Username = "user1", Email = "user1@gmail.com", PasswordHash = "password1" },
            new User { Id = 2, ProfileName = "User 2", Username = "user2", Email = "user2@gmail.com", PasswordHash = "password2" },
            new User { Id = 3, ProfileName = "User 3", Username = "user3", Email = "user3@gmail.com", PasswordHash = "password3" },
            new User { Id = 4, ProfileName = "User 4", Username = "user4", Email = "user4@gmail.com", PasswordHash = "password4" },
            new User { Id = 5, ProfileName = "User 5", Username = "user5", Email = "user5@gmail.com", PasswordHash = "password5" },
            new User { Id = 6, ProfileName = "User 6", Username = "user6", Email = "user6@gmail.com", PasswordHash = "password6" },
            new User { Id = 7, ProfileName = "User 7", Username = "user7", Email = "user7@gmail.com", PasswordHash = "password7" },
            new User { Id = 8, ProfileName = "User 8", Username = "user8", Email = "user8@gmail.com", PasswordHash = "password8" },
            new User { Id = 9, ProfileName = "User 9", Username = "user9", Email = "user9@gmail.com", PasswordHash = "password9" },
            new User { Id = 10, ProfileName = "User 10", Username = "user10", Email = "user10@gmail.com", PasswordHash = "password10" }
        };

        _tags = new List<Tag>
        {
            new Tag { Id = 1, TagName = "Memes" },
            new Tag { Id = 2, TagName = "Gain" },
            new Tag { Id = 3, TagName = "Loss" },
            new Tag { Id = 4, TagName = "Stocks" },
            new Tag { Id = 5, TagName = "Crypto" }
        };

        // Set up the to-one navigation property for Post to User
        _posts.ForEach(p => p.User = _users.FirstOrDefault(u => u.Id == p.UserId)!);

        // Set up the to-many navigation property for User to Post
        _users.ForEach(u => u.Posts = _posts.Where(p => p.UserId == u.Id).ToList());

        // Set up the many-to-many relationship between Posts and Tags
        // Ensure the first 10 posts have one tag each
        _posts[0].Tags.Add(_tags[0]); // Post 1 -> Tag 1 (Memes)
        _posts[0].Tags.Add(_tags[1]); // Post 1 -> Tag 2 (Gain)
        _posts[1].Tags.Add(_tags[1]); // Post 2 -> Tag 2 (Gain)
        _posts[0].Tags.Add(_tags[2]); // Post 1 -> Tag 3 (Loss)
        _posts[1].Tags.Add(_tags[2]); // Post 2 -> Tag 3 (Loss)
        _posts[2].Tags.Add(_tags[2]); // Post 3 -> Tag 3 (Loss)
        _posts[0].Tags.Add(_tags[3]); // Post 1 -> Tag 4 (Stocks)
        _posts[1].Tags.Add(_tags[3]); // Post 2 -> Tag 4 (Stocks)
        _posts[2].Tags.Add(_tags[3]); // Post 3 -> Tag 4 (Stocks)
        _posts[3].Tags.Add(_tags[3]); // Post 4 -> Tag 4 (Stocks)
        _posts[0].Tags.Add(_tags[4]); // Post 1 -> Tag 5 (Crypto)
        _posts[1].Tags.Add(_tags[4]); // Post 2 -> Tag 5 (Crypto)
        _posts[2].Tags.Add(_tags[4]); // Post 3 -> Tag 5 (Crypto)
        _posts[3].Tags.Add(_tags[4]); // Post 4 -> Tag 5 (Crypto)
        _posts[4].Tags.Add(_tags[4]); // Post 5 -> Tag 5 (Crypto)

        _tags[0].Posts.Add(_posts[0]); // Tag 1 (Memes) -> Post 1
        _tags[1].Posts.Add(_posts[0]); // Tag 2 (Gain) -> Post 1
        _tags[1].Posts.Add(_posts[1]); // Tag 2 (Gain) -> Post 2
        _tags[2].Posts.Add(_posts[0]); // Tag 3 (Loss) -> Post 1
        _tags[2].Posts.Add(_posts[1]); // Tag 3 (Loss) -> Post 2
        _tags[2].Posts.Add(_posts[2]); // Tag 3 (Loss) -> Post 3
        _tags[3].Posts.Add(_posts[0]); // Tag 4 (Stocks) -> Post 1
        _tags[3].Posts.Add(_posts[1]); // Tag 4 (Stocks) -> Post 2
        _tags[3].Posts.Add(_posts[2]); // Tag 4 (Stocks) -> Post 3
        _tags[3].Posts.Add(_posts[3]); // Tag 4 (Stocks) -> Post 4
        _tags[4].Posts.Add(_posts[0]); // Tag 5 (Crypto) -> Post 1
        _tags[4].Posts.Add(_posts[1]); // Tag 5 (Crypto) -> Post 2
        _tags[4].Posts.Add(_posts[2]); // Tag 5 (Crypto) -> Post 3
        _tags[4].Posts.Add(_posts[3]); // Tag 5 (Crypto) -> Post 4
        _tags[4].Posts.Add(_posts[4]); // Tag 5 (Crypto) -> Post 5

        _mockDbContext = new Mock<TrustTradeDbContext>();
        _mockPostDbSet = GetMockDbSet(_posts.AsQueryable());
        _mockDbContext.Setup(c => c.Posts).Returns(_mockPostDbSet.Object);
        _mockDbContext.Setup(c => c.Set<Post>()).Returns(_mockPostDbSet.Object); 
    }

    [Test]
    public void GetTotalPosts_WhenNoParametersGiven_ReturnsTotalNumberOfPosts()
    {
        // Arrange
        IPostRepository postRepository = new PostRepository(_mockDbContext.Object);
        var expected = _posts.Count;

        // Act
        var result = postRepository.GetTotalPosts();

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("Memes", 1)]
    [TestCase("Gain", 2)]
    [TestCase("Loss", 3)]
    [TestCase("Stocks", 4)]
    [TestCase("Crypto", 5)]
    public void GetTotalPosts_WhenCategoryFilterUsed_ReturnsTotalNumberOfPostsInCategory(string category, int expected)
    {
        // Arrange
        IPostRepository postRepository = new PostRepository(_mockDbContext.Object);

        // Act
        var result = postRepository.GetTotalPosts(category);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetPagedPosts_WhenNoParametersGiven_Returns10PostsSortedByDateDesc()
    {
        // Arrange
        IPostRepository postRepository = new PostRepository(_mockDbContext.Object);

        // Act
        // By default, category = null, sortOrder = "DateDesc", page=1, pageSize=10
        var results = postRepository.GetPagedPosts();

        // Assert
        Assert.That(results.Count, Is.EqualTo(10), "Should return 10 posts by default.");
        // Verify descending order by CreatedAt
        for (int i = 0; i < results.Count - 1; i++)
        {
            Assert.That(
                results[i].CreatedAt >= results[i + 1].CreatedAt,
                $"Posts are not sorted by descending CreatedAt at index {i}."
            );
        }
    }

    [Test]
    public void GetPagedPosts_WhenDateAscSortOrderUsed_Returns10PostsSortedByDateAsc()
    {
        // Arrange
        IPostRepository postRepository = new PostRepository(_mockDbContext.Object);

        // Act
        var results = postRepository.GetPagedPosts(page: 1, pageSize: 10, sortOrder: "DateAsc");

        // Assert
        Assert.That(results.Count, Is.EqualTo(10), "Should return 10 posts by default page size.");
        // Verify ascending order by CreatedAt
        for (int i = 0; i < results.Count - 1; i++)
        {
            Assert.That(
                results[i].CreatedAt <= results[i + 1].CreatedAt,
                $"Posts are not sorted by ascending CreatedAt at index {i}."
            );
        }
    }

    [Test]
    public void GetPagedPosts_WhenTitleAscSortOrderUsed_Returns10PostsSortedByTitleAsc()
    {
        // Arrange
        IPostRepository postRepository = new PostRepository(_mockDbContext.Object);

        // Act
        var results = postRepository.GetPagedPosts(page: 1, pageSize: 10, sortOrder: "TitleAsc");

        // Assert
        Assert.That(results.Count, Is.EqualTo(10), "Should return 10 posts by default page size.");
        // Verify ascending order by Title
        for (int i = 0; i < results.Count - 1; i++)
        {
            Assert.That(
                string.Compare(results[i].Title, results[i + 1].Title, StringComparison.Ordinal) <= 0,
                $"Posts are not sorted by ascending Title at index {i}."
            );
        }
    }

    [TestCase("Memes", 1)]
    [TestCase("Gain", 2)]
    [TestCase("Loss", 3)]
    [TestCase("Stocks", 4)]
    [TestCase("Crypto", 5)]
    [TestCase("memes", 1)] // Case insensitivity check
    [TestCase("GAIN", 2)]  // Case insensitivity check
    public void GetPagedPosts_WhenCategoryFilterUsed_ReturnsPostsInCategory(string categoryName, int expectedCount)
    {
        // Arrange
        IPostRepository postRepository = new PostRepository(_mockDbContext.Object);

        // Act
        var results = postRepository.GetPagedPosts(category: categoryName);

        // Assert
        Assert.That(results.Count, Is.EqualTo(expectedCount), "Should return expected number of posts in category.");
        
        // Verify all posts have the expected category
        foreach (var post in results)
        {
            var tagNames = post.Tags.Select(t => t.TagName).ToList();
            Assert.That(tagNames, Has.Some.EqualTo(categoryName).IgnoreCase, $"Post does not have the expected category: {categoryName}");
        }
    }
}