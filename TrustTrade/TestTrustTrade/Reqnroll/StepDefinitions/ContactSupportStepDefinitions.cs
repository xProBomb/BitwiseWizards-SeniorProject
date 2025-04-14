using Moq;
using NUnit.Framework;
using Reqnroll; // Formerly SpecFlow
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures; // Required for TempData
using Microsoft.AspNetCore.Http; // Required for TempData
using TrustTrade.ViewModels;
using TrustTrade.Services; // Assuming EmailSender is here
// Make sure your SupportController is accessible, adjust namespace if needed
// using TrustTrade.Controllers; // If it's in the main project

namespace TestTrustTrade.StepDefinitions
{
    [Binding]
    public class ContactSupportStepDefinitions
    {
        // Shared context between steps
        private readonly ScenarioContext _scenarioContext;
        private Mock<IEmailSender> _emailSenderMock;
        private Mock<UserManager<IdentityUser>> _userManagerMock;
        private SupportController _controller;
        private IActionResult _actionResult;
        private ContactSupportViewModel _viewModel;
        private IdentityUser _currentUser;

        // Constructor for dependency injection (Reqnroll handles this)
        public ContactSupportStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;

            // Initialize mocks ONCE per scenario using BeforeScenario or here if simple
            _emailSenderMock = new Mock<IEmailSender>();

            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _controller = new SupportController(_emailSenderMock.Object, _userManagerMock.Object);

            // Setup TempData - Essential for verifying the success message
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            // Initialize ViewModel for the scenario
            _viewModel = new ContactSupportViewModel();
        }

        [Given(@"the user ""(.*)"" is logged in")]
        public void GivenTheUserIsLoggedIn(string userEmail)
        {
            _currentUser = new IdentityUser { UserName = userEmail, Email = userEmail };

            // Mock GetUserAsync to return our test user
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync(_currentUser);

            // Simulate the user being authenticated in the controller context (important for [Authorize])
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, _currentUser.Id ?? Guid.NewGuid().ToString()), // Add necessary claims
                new Claim(ClaimTypes.Email, _currentUser.Email),
                // Add other claims if your controller uses them
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // Store user for potential later use
             _scenarioContext["CurrentUser"] = _currentUser;
        }

        [Given(@"the user navigates to the Contact Support page")]
        public void GivenTheUserNavigatesToTheContactSupportPage()
        {
            // Call the GET action to simulate navigation
            _actionResult = _controller.ContactSupport();
            // We can assert here if needed, but often the main check is the POST
             Assert.That(_actionResult, Is.InstanceOf<ViewResult>(), "Navigating to Contact Support should return a ViewResult.");
        }

        [When(@"the user enters ""(.*)"" into the Name field")]
        public void WhenTheUserEntersIntoTheNameField(string name)
        {
            _viewModel.Name = name;
        }

        [When(@"the user selects ""(.*)"" as the Category")]
        public void WhenTheUserSelectsAsTheCategory(string category)
        {
            _viewModel.Tag = category; // ViewModel uses 'Tag' for category
        }

        [When(@"the user enters ""(.*)"" into the Message field")]
        public void WhenTheUserEntersIntoTheMessageField(string message)
        {
            _viewModel.Message = message;
        }

        [When(@"the user submits the support form")]
        public async Task WhenTheUserSubmitsTheSupportForm()
        {
            // Simulate POST request
            _actionResult = await _controller.ContactSupport(_viewModel);
        }

        [Then(@"an email should be sent to the support address ""(.*)"" with category ""(.*)""")]
        public void ThenAnEmailShouldBeSentToTheSupportAddressWithCategory(string expectedEmail, string expectedCategory)
        {
            // Verify SendEmailAsync was called correctly
            _emailSenderMock.Verify(x => x.SendEmailAsync(
                expectedEmail, // The fixed support email address
                It.Is<string>(subject => subject.Contains(expectedCategory)), // Subject contains the category/tag
                It.Is<string>(body => // Body contains user email, name, and message
                    body.Contains(_currentUser.Email) &&
                    body.Contains(_viewModel.Name) &&
                    body.Contains(_viewModel.Message)
                )
            ), Times.Once, "Email sending was not verified with the expected parameters.");
        }

        [Then(@"the user should be redirected back to the Contact Support page")]
        public void ThenTheUserShouldBeRedirectedBackToTheContactSupportPage()
        {
            var redirectResult = _actionResult as RedirectToActionResult;
             Assert.That(redirectResult, Is.Not.Null, "Expected the action result to be a RedirectToActionResult.");
             Assert.That(redirectResult.ActionName, Is.EqualTo("ContactSupport"), "Expected redirection to the 'ContactSupport' action.");
             // Optionally check controller name if needed: Assert.That(redirectResult.ControllerName, Is.EqualTo("Support"));
        }

        [Then(@"a success message ""(.*)"" should be set")]
        public void ThenASuccessMessageShouldBeSet(string expectedMessage)
        {
            // Verify TempData was set correctly BEFORE the redirect happens
             Assert.That(_controller.TempData["SuccessMessage"], Is.EqualTo(expectedMessage), "TempData success message mismatch.");
        }
    }
}