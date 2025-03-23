using Moq;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using TrustTrade.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using TrustTrade.ViewModels;
using System.Threading.Tasks;

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

    private User _user1;
    private User _user2;
    private Post _postByUser1;
    private CreatePostVM _validCreatePostVM;
    private CreatePostVM _invalidCreatePostVM;
    private PostEditVM _validPostEditVM;
    private PostEditVM _invalidPostEditVM;

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

        
        // Test users
        _user1 = new User
        {
            Id = 1,
            IdentityId = "test-identity-1",
            ProfileName = "johnDoe",
            Username = "johnDoe",
            Email = "johndoe@example.com",
            PasswordHash = "dummyHash"
        };

        _user2 = new User
        {
            Id = 2,
            IdentityId = "test-identity-2",
            ProfileName = "janeDoe",
            Username = "janeDoe",
            Email = "janedoe@example.com",
            PasswordHash = "dummyHash"
        };

        // Test posts
        _postByUser1 = new Post
        {
            Id = 1,
            UserId = 1,
            Title = "Test Post",
            Content = "Test Content",
            CreatedAt = DateTime.Now,
            IsPublic = true,
            User = _user1,
        };

        // Test view models
        _validCreatePostVM= new CreatePostVM
        {
            Title = "Test Post", 
            Content = "Test Content",
        };

        _invalidCreatePostVM = new CreatePostVM
        {
            Title = "", // Invalid: Title is required
            Content = "Test Content"
        };

        _validPostEditVM = new PostEditVM
        {
            Id = 1,
            Title = "Updated Post",
            Content = "Updated Content",
            IsPublic = false,
            SelectedTags = new List<string> { "Memes", "Gain", "Loss" }
        };

        _invalidPostEditVM = new PostEditVM
        {
            Id = 1,
            Title = "", // Invalid: Title is required
            Content = "Updated Content",
            IsPublic = false,
            SelectedTags = new List<string> { "Memes", "Gain", "Loss" }
        };
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
    }

    [Test]
    public async Task CreateGET_ReturnsViewResult()
    {
        // Arrange
        _tagRepositoryMock.Setup(r => r.GetAllTagNamesAsync()).ReturnsAsync(new List<string>());

        // Act
        var result = await _controller.Create();

        // Assert
        Assert.That(result, Is.Not.Null.And.TypeOf<ViewResult>());
    }

    [Test]
    public async Task CreateGET_IncludesModelOfTypeCreatePostVM()
    {
        // Arrange
        _tagRepositoryMock.Setup(r => r.GetAllTagNamesAsync()).ReturnsAsync(new List<string>());

        // Act
        var result = await _controller.Create() as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.Not.Null.And.TypeOf<CreatePostVM>());
    }

    [Test]
    public async Task CreateGET_WhenTagsExist_PopulatesExistingTags()
    {
        // Arrange
        var tags = new List<string> { "Memes", "Gain", "Loss", "Stocks", "Crypto" };
        _tagRepositoryMock.Setup(r => r.GetAllTagNamesAsync()).ReturnsAsync(tags);

        // Act
        var result = await _controller.Create() as ViewResult;
        var model = result?.Model as CreatePostVM;

        // Assert
        Assert.That(model, Is.Not.Null);
        Assert.That(model.ExistingTags, Is.EquivalentTo(tags));
    }

    [Test]
    public async Task CreateGET_WhenNoTagsExist_SetsEmptyTagList()
    {
        // Arrange
        _tagRepositoryMock.Setup(r => r.GetAllTagNamesAsync()).ReturnsAsync(new List<string>());

        // Act
        var result = await _controller.Create() as ViewResult;
        var model = result?.Model as CreatePostVM;

        // Assert
        Assert.That(model, Is.Not.Null);
        Assert.That(model.ExistingTags, Is.Empty);
    }

    [Test]
    public async Task CreatePOST_WhenValid_SavesPostAndRedirectsToIndex()
    {
        // Arrange
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("test-identity-1");
        _userRepositoryMock.Setup(r => r.FindByIdentityIdAsync(It.IsAny<string>())).ReturnsAsync(_user1);
        _postRepositoryMock.Setup(r => r.AddOrUpdate(It.IsAny<Post>())).Verifiable();

        // Act
        var result = await _controller.Create(_validCreatePostVM) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"), "Expected to redirect to 'Index'");

         _postRepositoryMock.Verify(r => r.AddOrUpdate(It.IsAny<Post>()), Times.Once);
    }

    [Test]
    public async Task CreatePOST_WhenTagsSelected_FindsTags()
    {
        // Arrange
        var vm = new CreatePostVM
        {
            Title = "Test Post", 
            Content = "Test Content",
            SelectedTags = new List<string> { "Memes", "Gain" }
        };

        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("test-identity-1");
        _userRepositoryMock.Setup(r => r.FindByIdentityIdAsync(It.IsAny<string>())).ReturnsAsync(_user1);
        _tagRepositoryMock.Setup(r => r.FindByTagNameAsync(It.IsAny<string>())).ReturnsAsync(new Tag());
        _postRepositoryMock.Setup(r => r.AddOrUpdate(It.IsAny<Post>())).Verifiable();

        // Act
        var result = await _controller.Create(vm) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"), "Expected to redirect to 'Index'");

        _tagRepositoryMock.Verify(r => r.FindByTagNameAsync("Memes"), Times.Once);
        _tagRepositoryMock.Verify(r => r.FindByTagNameAsync("Gain"), Times.Once);
        _postRepositoryMock.Verify(r => r.AddOrUpdate(It.IsAny<Post>()), Times.Once);
    }



    [Test]
    public async Task CreatePOST_WhenPlaidEnabled_GetsHoldingsAndRedirectsToIndex()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            IdentityId = "test-identity-1",
            ProfileName = "johnDoe",
            Username = "johnDoe",
            Email = "johndoe@example.com",
            PasswordHash = "dummyHash",
            PlaidEnabled = true
        };

        var investmentPositions = new List<InvestmentPosition>
        {
            new InvestmentPosition { Id = 1, PlaidConnectionId = 1, SecurityId= "test-security-id-1", Symbol = "AAPL", Quantity = 10, CostBasis = 100, CurrentPrice = 110, TypeOfSecurity = "Stock" },
            new InvestmentPosition { Id = 2, PlaidConnectionId = 1, SecurityId= "test-security-id-2", Symbol = "TSLA", Quantity = 5, CostBasis = 200, CurrentPrice = 220, TypeOfSecurity = "Stock" }
        };

        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("test-identity-1");
        _userRepositoryMock.Setup(r => r.FindByIdentityIdAsync(It.IsAny<string>())).ReturnsAsync(user);
        _holdingsRepositoryMock.Setup(r => r.RefreshHoldingsAsync(It.IsAny<int>())).ReturnsAsync(true);
        _holdingsRepositoryMock.Setup(r => r.GetHoldingsForUserAsync(It.IsAny<int>())).ReturnsAsync(investmentPositions).Verifiable();
        _postRepositoryMock.Setup(r => r.AddOrUpdate(It.IsAny<Post>())).Verifiable();

        // Act
        var result = await _controller.Create(_validCreatePostVM) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"), "Expected to redirect to 'Index'");

        _holdingsRepositoryMock.Verify(r => r.GetHoldingsForUserAsync(It.IsAny<int>()), Times.Once);
        _postRepositoryMock.Verify(r => r.AddOrUpdate(It.IsAny<Post>()), Times.Once);
    }

    [Test]
    public async Task CreatePOST_WhenIdentityUserNotFound_ReturnsUnauthorized()
    {
        // Arrange
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(() => null);

        // Act
        var result = await _controller.Create(_validCreatePostVM);

        // Assert
        Assert.That(result, Is.Not.Null.And.TypeOf<UnauthorizedResult>());
    }

    [Test]
    public async Task CreatePOST_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("test-identity-1");
        _userRepositoryMock.Setup(r => r.FindByIdentityIdAsync(It.IsAny<string>())).ReturnsAsync(() => null);

        // Act
        var result = await _controller.Create(_validCreatePostVM);

        // Assert
        Assert.That(result, Is.Not.Null.And.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task CreatePOST_WhenInvalid_ReturnsViewResult()
    {
        // Arrange
        _controller.ModelState.AddModelError("Title", "Title is required");
        _tagRepositoryMock.Setup(r => r.GetAllTagNamesAsync()).ReturnsAsync(new List<string>());

        // Act
        var result = await _controller.Create(_invalidCreatePostVM);

        // Assert
        Assert.That(result, Is.Not.Null.And.TypeOf<ViewResult>());
    }

    [Test]
    public async Task CreatePOST_WhenInvalid_IncludesModelOfTypeCreatePostVM()
    {
        // Arrange
        _controller.ModelState.AddModelError("Title", "Title is required");
        _tagRepositoryMock.Setup(r => r.GetAllTagNamesAsync()).ReturnsAsync(new List<string>());

        // Act
        var result = await _controller.Create(_invalidCreatePostVM) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.Not.Null.And.TypeOf<CreatePostVM>());
    }

    [Test]
    public async Task CreatePOST_WhenInvalidAndTagsExist_PopulatesExistingTags()
    {
        // Arrange
        var tags = new List<string> { "Memes", "Gain", "Loss", "Stocks", "Crypto" };

        _tagRepositoryMock.Setup(r => r.GetAllTagNamesAsync()).ReturnsAsync(tags);
        _controller.ModelState.AddModelError("Title", "Title is required");

        // Act
        var result = await _controller.Create(_invalidCreatePostVM) as ViewResult;
        var model = result?.Model as CreatePostVM;

        // Assert
        Assert.That(model, Is.Not.Null);
        Assert.That(model.ExistingTags, Is.EquivalentTo(tags));
    }

    [Test]
    public async Task CreatePOST_WhenInvalidAndNoTagsExist_SetsEmptyTagList()
    {
        // Arrange
        _tagRepositoryMock.Setup(r => r.GetAllTagNamesAsync()).ReturnsAsync(new List<string>());
        _controller.ModelState.AddModelError("Title", "Title is required");

        // Act
        var result = await _controller.Create(_invalidCreatePostVM) as ViewResult;
        var model = result?.Model as CreatePostVM;

        // Assert
        Assert.That(model, Is.Not.Null);
        Assert.That(model.ExistingTags, Is.Empty);
    }

    [Test]
    public async Task Details_WhenPostExists_ReturnsViewResult()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).Returns(_postByUser1);

        // Act
        var result = await _controller.Details(1);

        // Assert
        Assert.That(result, Is.Not.Null.And.TypeOf<ViewResult>());
    }

    [Test]
    public async Task Details_WhenPostExists_IncludesModelOfTypePostDetailsVM()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).Returns(_postByUser1);

        // Act
        var result = await _controller.Details(1) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.Not.Null.And.TypeOf<PostDetailsVM>());
    }

    [Test]
    public async Task Details_WhenPostExists_ReturnsViewModelWithPostDetails()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).Returns(_postByUser1);

        // Act
        var result = await _controller.Details(1) as ViewResult;
        var model = result?.Model as PostDetailsVM;

        // Assert
        Assert.That(model, Is.Not.Null);
        Assert.That(model.Title, Is.EqualTo("Test Post"));
        Assert.That(model.Content, Is.EqualTo("Test Content"));
        Assert.That(model.Username, Is.EqualTo("johnDoe"));
        Assert.That(model.Tags, Is.Empty);
        Assert.That(model.LikeCount, Is.Zero);
        Assert.That(model.CommentCount, Is.Zero);
        Assert.That(model.IsPlaidEnabled, Is.False);
        Assert.That(model.PortfolioValueAtPosting, Is.Null);

    }

    [Test]
    public async Task Details_WhenUserIsPlaidEnabled_ReturnsViewModelWithPortfolioValue()
    {
        // Arrange
        var post = new Post
        {
            Id = 1,
            Title = "Test Post",
            Content = "Test Content",
            CreatedAt = DateTime.Now,
            IsPublic = true,
            PortfolioValueAtPosting = 1000,
            User = new User { Username = "johnDoe", PlaidEnabled = true },
        };

        _postRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).Returns(post);

        // Act
        var result = await _controller.Details(1) as ViewResult;
        var model = result?.Model as PostDetailsVM;

        // Assert
        Assert.That(model, Is.Not.Null);
        Assert.That(model.PortfolioValueAtPosting, Is.EqualTo("$1K"));
        Assert.That(model.IsPlaidEnabled, Is.True);
    }

    [Test]
    public async Task Details_WhenCurrentUserIsOwner_ReturnsViewModelShowingOwnership()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).Returns(_postByUser1);
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("test-identity-1");
        _userRepositoryMock.Setup(r => r.FindByIdentityIdAsync(It.IsAny<string>())).ReturnsAsync(_user1);

        // Act
        var result = await _controller.Details(1) as ViewResult;
        var model = result?.Model as PostDetailsVM;

        // Assert
        Assert.That(model, Is.Not.Null);
        Assert.That(model.IsOwnedByCurrentUser, Is.True);
    }

    [Test]
    public async Task Details_WhenPostDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).Returns(() => null!);

        // Act
        var result = await _controller.Details(1);

        // Assert
        Assert.That(result, Is.Not.Null.And.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task EditGET_WhenUserIsOwner_ReturnsViewResultWithPostEditVM()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).Returns(_postByUser1);
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("test-identity-1");
        _userRepositoryMock.Setup(r => r.FindByIdentityIdAsync(It.IsAny<string>())).ReturnsAsync(_user1);
        _tagRepositoryMock.Setup(r => r.GetAllTagNamesAsync()).ReturnsAsync(new List<string>());

        // Act
        var result = await _controller.Edit(1) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<ViewResult>());
        Assert.That(result.Model, Is.Not.Null.And.TypeOf<PostEditVM>());
    }

    [Test]
    public async Task EditGET_WhenUserIsOwner_ReturnsViewModelWithPostDetails()
    {
        // Arrange
        var tags = new List<Tag>
        {
            new Tag { Id = 1, TagName = "Memes" },
            new Tag { Id = 2, TagName = "Gain" }
        };

        var post = new Post
        {
            Id = 1,
            UserId = 1,
            Title = "Test Post",
            Content = "Test Content",
            CreatedAt = DateTime.Now,
            IsPublic = true,
            User = _user1,
            Tags = tags
        };

        var availableTags = new List<string> { "Memes", "Gain", "Loss", "Stocks", "Crypto" };

        _postRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).Returns(post);
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("test-identity-1");
        _userRepositoryMock.Setup(r => r.FindByIdentityIdAsync(It.IsAny<string>())).ReturnsAsync(_user1);
        _tagRepositoryMock.Setup(r => r.GetAllTagNamesAsync()).ReturnsAsync(availableTags);

        // Act
        var result = await _controller.Edit(1) as ViewResult;
        var model = result?.Model as PostEditVM;

        // Assert
        Assert.That(model, Is.Not.Null);
        Assert.That(model.Id, Is.EqualTo(1));
        Assert.That(model.Title, Is.EqualTo("Test Post"));
        Assert.That(model.Content, Is.EqualTo("Test Content"));
        Assert.That(model.IsPublic, Is.True);
        Assert.That(model.AvailableTags, Is.EquivalentTo(availableTags));
        Assert.That(model.SelectedTags, Is.EquivalentTo(new List<string> { "Memes", "Gain" }));
    }

    [Test]
    public async Task EditGET_WhenUserIsNotOwner_ReturnsUnauthorized()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).Returns(_postByUser1);
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("test-identity-2");
        _userRepositoryMock.Setup(r => r.FindByIdentityIdAsync(It.IsAny<string>())).ReturnsAsync(_user2);

        // Act
        var result = await _controller.Edit(1);

        // Assert
        Assert.That(result, Is.Not.Null.And.TypeOf<UnauthorizedResult>());
    }

    [Test]
    public async Task EditGET_WhenNotLoggedIn_ReturnsUnauthorized()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).Returns(_postByUser1);
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(() => null);

        // Act
        var result = await _controller.Edit(1);

        // Assert
        Assert.That(result, Is.Not.Null.And.TypeOf<UnauthorizedResult>());
    }

    [Test]
    public async Task EditGET_WhenPostDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).Returns(() => null!);

        // Act
        var result = await _controller.Edit(1);

        // Assert
        Assert.That(result, Is.Not.Null.And.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task EditPOST_WhenUserIsOwner_UpdatesPostAndRedirectsToDetails()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).Returns(_postByUser1);
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("test-identity-1");
        _userRepositoryMock.Setup(r => r.FindByIdentityIdAsync(It.IsAny<string>())).ReturnsAsync(_user1);
        _tagRepositoryMock.Setup(r => r.FindByTagNameAsync(It.IsAny<string>())).ReturnsAsync(new Tag());
        _postRepositoryMock.Setup(r => r.AddOrUpdate(It.IsAny<Post>())).Verifiable();

        // Act
        var result = await _controller.Edit(1, _validPostEditVM) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Details"), "Expected to redirect to 'Details'");

        _postRepositoryMock.Verify(r => r.AddOrUpdate(It.IsAny<Post>()), Times.Once);
    }

    [Test]
    public async Task EditPOST_WhenUserIsNotOwner_ReturnsUnauthorized()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).Returns(_postByUser1);
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("test-identity-2");
        _userRepositoryMock.Setup(r => r.FindByIdentityIdAsync(It.IsAny<string>())).ReturnsAsync(_user2);

        // Act
        var result = await _controller.Edit(1, _validPostEditVM);

        // Assert
        Assert.That(result, Is.Not.Null.And.TypeOf<UnauthorizedResult>());
    }

    [Test]
    public async Task EditPOST_WhenNotLoggedIn_ReturnsUnauthorized()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).Returns(_postByUser1);
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(() => null);

        // Act
        var result = await _controller.Edit(1, _validPostEditVM);

        // Assert
        Assert.That(result, Is.Not.Null.And.TypeOf<UnauthorizedResult>());
    }

    [Test]
    public async Task EditPOST_WhenPostDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).Returns(() => null!);

        // Act
        var result = await _controller.Edit(1, _validPostEditVM);

        // Assert
        Assert.That(result, Is.Not.Null.And.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task EditPOST_WhenInvalid_ReturnsViewResultWithPostEditVM()
    {
        // Arrange
        _controller.ModelState.AddModelError("Title", "Title is required");
        _tagRepositoryMock.Setup(r => r.GetAllTagNamesAsync()).ReturnsAsync(new List<string>());

        // Act
        var result = await _controller.Edit(1, _invalidPostEditVM) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<ViewResult>());
        Assert.That(result.Model, Is.Not.Null.And.TypeOf<PostEditVM>());
    }

    [Test]
    public async Task DeletePOST_WhenUserIsOwner_DeletesPostAndRedirectsToIndex()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).Returns(_postByUser1);
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("test-identity-1");
        _userRepositoryMock.Setup(r => r.FindByIdentityIdAsync(It.IsAny<string>())).ReturnsAsync(_user1);
        _postRepositoryMock.Setup(r => r.Delete(It.IsAny<Post>())).Verifiable();

        // Act
        var result = await _controller.Delete(1) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"), "Expected to redirect to 'Index'");

        _postRepositoryMock.Verify(r => r.Delete(It.IsAny<Post>()), Times.Once);
    }

    [Test]
    public async Task DeletePOST_WhenUserIsNotOwner_ReturnsUnauthorized()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).Returns(_postByUser1);
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("test-identity-2");
        _userRepositoryMock.Setup(r => r.FindByIdentityIdAsync(It.IsAny<string>())).ReturnsAsync(_user2);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.That(result, Is.Not.Null.And.TypeOf<UnauthorizedResult>());
    }

    [Test]
    public async Task DeletePOST_WhenNotLoggedIn_ReturnsUnauthorized()
    {
        // Arrange
        _postRepositoryMock.Setup(r => r.FindById(It.IsAny<int>())).Returns(_postByUser1);
        _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(() => null);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.That(result, Is.Not.Null.And.TypeOf<UnauthorizedResult>());
    }
}
