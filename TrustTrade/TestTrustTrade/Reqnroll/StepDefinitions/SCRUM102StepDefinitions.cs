using Reqnroll;
using TestTrustTrade.Reqnroll.Services;
using TestTrustTrade.Reqnroll.Pages;

namespace TestTrustTrade.Reqnroll.StepDefinitions
{
    [Binding]
    public class SCRUM102StepDefinitions
    {
        private LoginService _loginService;
        private PostPage _postPage;

        [BeforeScenario]
        public void Setup()
        {
            _loginService = new LoginService();
            _postPage = new PostPage(1);
        }

        [AfterScenario]
        public void AfterScenario()
        {
            _loginService.Logout();
            WebDriverSingleton.ReleaseInstance();
        }

        [Given(@"I am on the page for a post")]
        public void GivenIAmOnThePageForAPost()
        {
            _postPage.NavigateTo();
        }

        [Given(@"I see a comment")]
        public void ISeeAComment()
        {
            var isCommentVisible = _postPage.IsCommentVisible();
            Assert.That(isCommentVisible, Is.True);
        }

        [Then(@"the like button should be visible on the comment")]
        public void ThenTheLikeButtonShouldBeVisibleOnTheComment()
        {
            var isLikeButtonVisible = _postPage.IsLikeButtonVisible();
            Assert.That(isLikeButtonVisible, Is.True);
        }

        [Given(@"I am logged in")]
        public void GivenIAmALoggedInUser()
        {
            bool isLoggedIn = _loginService.Login(LoginService.User1);
            Assert.That(isLoggedIn, Is.True);
        }


        [Given(@"the comment has a like button")]
        public void GivenTheCommentHasALikeButton()
        {
            var isLikeButtonVisible = _postPage.IsLikeButtonVisible();
            Assert.That(isLikeButtonVisible, Is.True);
        }

        [Given(@"the comment has not been liked by me")]
        public void GivenTheCommentHasNotBeenLikedByMe()
        {
            var isCommentLiked = _postPage.IsCommentLiked();
            Assert.That(isCommentLiked, Is.False);
        }

        [When(@"I click the like button on the comment")]
        public void WhenIClickTheLikeButtonOnTheComment()
        {
            _postPage.ClickCommentLikeButton();
        }

        [Then(@"the comment should be marked as liked")]
        public void ThenTheCommentShouldBeMarkedAsLiked()
        {
            var isCommentLiked = _postPage.HasCommentTransitionedToLiked();
            Assert.That(isCommentLiked, Is.True);
        }

        [Given(@"the comment has been liked by me")]
        public void GivenTheCommentHasBeenLikedByMe()
        {
            var isCommentLiked = _postPage.IsCommentLiked();
            Assert.That(isCommentLiked, Is.True);
        }

        [When(@"I click the like button on the comment again")]
        public void WhenIClickTheLikeButtonOnTheCommentAgain()
        {
            _postPage.ClickCommentLikeButton();
        }

        [Then(@"the comment should be marked as unliked")]
        public void ThenTheCommentShouldBeMarkedAsUnliked()
        {
            var isCommentLiked = _postPage.HasCommentTransitionedToUnliked();
            Assert.That(isCommentLiked, Is.False);
        }
    }
}