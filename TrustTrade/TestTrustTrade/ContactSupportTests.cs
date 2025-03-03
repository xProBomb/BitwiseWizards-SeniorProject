using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Identity.UI.Services;
using Moq;
using NUnit.Framework;
using TrustTrade.Services;
using TrustTrade.ViewModels;

namespace TrustTrade.Tests
{
    [TestFixture]
    public class SupportControllerTests
    {
        private Mock<IEmailSender> _emailSenderMock;
        private Mock<UserManager<IdentityUser>> _userManagerMock;
        private SupportController _controller;

        [SetUp]
        public void Setup()
        {
            _emailSenderMock = new Mock<IEmailSender>();

            // Setup UserManager mock using a dummy IUserStore.
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // Create the controller instance.
            _controller = new SupportController(_emailSenderMock.Object, _userManagerMock.Object);

            // Setup TempData.
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        }

        [TearDown]
        public void TearDown()
        {
            // Dispose of the controller to satisfy NUnit analyzer.
            _controller?.Dispose();
            _controller = null;
        }

        [Test]
        public void ContactSupport_Get_ReturnsView()
        {
            // Act
            var result = _controller.ContactSupport();

            // Assert using constraint-based syntax.
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task ContactSupport_Post_InvalidModel_ReturnsViewWithSameModel()
        {
            // Arrange: mark model state as invalid.
            _controller.ModelState.AddModelError("Name", "Name is required");
            var model = new ContactSupportViewModel { Name = "", Tag = "Bug", Message = "Test message" };

            // Act
            var result = await _controller.ContactSupport(model);

            // Assert.
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult.Model, Is.EqualTo(model));
        }

        [Test]
        public async Task ContactSupport_Post_ValidModel_SendsEmailAndRedirects()
        {
            // Arrange: set up a test user.
            var testUser = new IdentityUser { Email = "user@test.com" };

            // Setup GetUserAsync to return the test user.
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync(testUser);

            var model = new ContactSupportViewModel 
            { 
                Name = "John Doe", 
                Tag = "Question", 
                Message = "Please help me with my issue." 
            };

            // Act.
            var result = await _controller.ContactSupport(model);

            // Assert: Verify that SendEmailAsync was called with an email body that includes test data.
            _emailSenderMock.Verify(x => x.SendEmailAsync(
                "trusttrade.auth@gmail.com",
                It.Is<string>(subject => subject.Contains(model.Tag)),
                It.Is<string>(body =>
                    body.Contains(testUser.Email) &&
                    body.Contains(model.Name) &&
                    body.Contains(model.Message)
                )
            ), Times.Once);

            // Assert that the result is a redirect to the GET action.
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.ActionName, Is.EqualTo("ContactSupport"));

            // Verify TempData was set.
            Assert.That(_controller.TempData["SuccessMessage"],
                        Is.EqualTo("Your support request has been sent successfully."));
        }
    }
}
