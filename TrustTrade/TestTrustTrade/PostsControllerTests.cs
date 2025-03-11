using Moq;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using Microsoft.EntityFrameworkCore;
using TrustTrade.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using TrustTrade.ViewModels;

namespace TestTrustTrade;

[TestFixture]
public class PostsControllerTests
{
    private Mock<ILogger<PostsController>> _loggerMock;
    private Mock<UserManager<IdentityUser>> _userManagerMock;
    private Mock<IHoldingsRepository> _holdingsRepositoryMock;
    private Mock<IPostRepository> _postRepositoryMock;
    private Mock<ITagRepository> _tagRepositoryMock;
    private Mock<IUserRepository> _userRepositoryMock;
    private PostsController _controller;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<PostsController>>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            new Mock<IUserStore<IdentityUser>>().Object,
            null!, null!, null!, null!, null!, null!, null!, null!
        );
        _holdingsRepositoryMock = new Mock<IHoldingsRepository>();
        _postRepositoryMock = new Mock<IPostRepository>();
        _tagRepositoryMock = new Mock<ITagRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _controller = new PostsController(
            _loggerMock.Object,
            _userManagerMock.Object,
            _holdingsRepositoryMock.Object,
            _postRepositoryMock.Object,
            _tagRepositoryMock.Object,
            _userRepositoryMock.Object
        );
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
    }

    [Test]
    public void Create_Get_WhenCalled_ReturnsViewResult()
    {
        // Arrange
        _tagRepositoryMock.Setup(r => r.GetAllTagNames()).Returns(new List<string>());

        // Act
        var result = _controller.Create();

        // Assert
        Assert.That(result, Is.Not.Null.And.TypeOf<ViewResult>());
    }

    [Test]
    public void Create_Get_WhenCalled_ReturnsModelOfTypeCreatePostVM()
    {
        // Arrange
        _tagRepositoryMock.Setup(r => r.GetAllTagNames()).Returns(new List<string>());

        // Act
        var result = _controller.Create() as ViewResult;
        var model = result?.Model;

        // Assert
        Assert.That(model, Is.Not.Null.And.TypeOf<CreatePostVM>());
    }

    [Test]
    public void Create_Get_WhenTagsExist_ReturnsViewModelWithPopulatedTags()
    {
        // Arrange
        var tags = new List<string> { "Memes", "Gain", "Loss", "Stocks", "Crypto" };
        _tagRepositoryMock.Setup(r => r.GetAllTagNames()).Returns(tags);

        // Act
        var result = _controller.Create() as ViewResult;
        var model = result?.Model as CreatePostVM;

        // Assert
        Assert.That(model, Is.Not.Null);
        Assert.That(model.ExistingTags, Is.EquivalentTo(tags));
    }

    [Test]
    public void Create_Get_WhenNoTagsExist_ReturnsViewModelWithEmptyTagList()
    {
        // Arrange
        _tagRepositoryMock.Setup(r => r.GetAllTagNames()).Returns(new List<string>());

        // Act
        var result = _controller.Create() as ViewResult;
        var model = result?.Model as CreatePostVM;

        // Assert
        Assert.That(model, Is.Not.Null);
        Assert.That(model!.ExistingTags, Is.Empty);
    }
}
