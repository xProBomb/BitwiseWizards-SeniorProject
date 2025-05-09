using OpenQA.Selenium;

namespace TestTrustTrade.Reqnroll.Pages
{
    public class PostPage : BasePage
    {
        private readonly int _postId;

        // Locators
        private readonly By _postDetails = By.ClassName("post-details");
        private readonly By _commentItem = By.ClassName("comment-item");
        private readonly By _commentLikeButton = By.ClassName("comment-like-button");

        public PostPage(int postId) : base()
        {
            _postId = postId;
        }

        protected override string PageUrl => $"/Posts/Details/{_postId}";

        public override bool IsPageLoaded()
        {
            return WaitForElementVisible(_postDetails).Displayed;
        }

        public bool IsCommentVisible()
        {
            var comment = WaitForElementVisible(_commentItem);
            return comment.Displayed;
        }

        public bool IsLikeButtonVisible()
        {
            var likeButton = WaitForElementVisible(_commentLikeButton);
            return likeButton.Displayed;
        }

        public void ClickCommentLikeButton()
        {
            var likeButton = Driver.FindElement(_commentLikeButton);
            likeButton.Click();
        }

        public bool IsCommentLiked()
        {
            var likeButton = Driver.FindElement(_commentLikeButton);
            return likeButton.GetAttribute("class").Contains("liked");
        }

        public bool HasCommentTransitionedToLiked()
        {
            var likeButton = Driver.FindElement(_commentLikeButton);

            // Wait for the "liked" class to be added
            Wait.Until(d => likeButton.GetAttribute("class").Contains("liked"));
            return likeButton.GetAttribute("class").Contains("liked");
        }

        public bool HasCommentTransitionedToUnliked()
        {
            var likeButton = Driver.FindElement(_commentLikeButton);

            // Wait for the "liked" class to be removed
            Wait.Until(d => !likeButton.GetAttribute("class").Contains("liked"));
            return likeButton.GetAttribute("class").Contains("liked");
        }
    }
}

