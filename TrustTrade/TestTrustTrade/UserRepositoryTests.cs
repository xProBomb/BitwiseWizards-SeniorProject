using Moq;
using TrustTrade.DAL.Abstract;
using TrustTrade.DAL.Concrete;
using TrustTrade.Models;
using Microsoft.EntityFrameworkCore;

namespace TestTrustTrade;

[TestFixture]
public class UserRepositoryTests
{
    private TrustTradeDbContext _dbContext;
    private UserRepository _userRepository;


    [SetUp]
    public void Setup()
    {
        // Create a new in-memory database for each test
        var options = new DbContextOptionsBuilder<TrustTradeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        _dbContext = new TrustTradeDbContext(options);
        _userRepository = new UserRepository(_dbContext);


        var users = new List<User>
        {
            new User { Id = 1, IdentityId = "test-identity-1", ProfileName = "johnDoe", Username = "johnDoe", Email = "johndoe@example.com", PasswordHash = "dummyHash" },
            new User { Id = 2, IdentityId = "test-identity-2", ProfileName = "janeSmith", Username = "janeSmith", Email = "janesmith@example.com", PasswordHash = "dummyHash" }
        };

        // Seed the database with initial data
        _dbContext.Users.AddRange(users);
        _dbContext.SaveChanges();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public void FindByIdentityId_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var expectedId = 1;
        var expectedIdentityId = "test-identity-1";

        // Act
        var result = _userRepository.FindByIdentityId(expectedIdentityId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(expectedId));
        Assert.That(result.IdentityId, Is.EqualTo(expectedIdentityId));
    }

    [Test]
    public void FindByIdentityId_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var nonExistentIdentityId = "non-existent-identity-id";

        // Act
        var result = _userRepository.FindByIdentityId(nonExistentIdentityId);

        // Assert
        Assert.That(result, Is.Null);
    }
}