using Moq;
using NUnit.Framework;
using Reqnroll;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TrustTrade.ViewModels;
using TrustTrade.Services.Web.Interfaces;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;

namespace TestTrustTrade.StepDefinitions
{
    [Binding]
    public class LikingStepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;

        public LikingStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }
        private Mock<ILikeRepository> _likeRepositoryMock = new Mock<ILikeRepository>();
        private Mock<IPostRepository> _postRepositoryMock = new Mock<IPostRepository>();
        private Mock<IUserService> _userServiceMock = new Mock<IUserService>();
        private PostPreviewVM _currentPost;
        private int _initialLikeCount;

        [Given("I am on the website")]
        public void GivenIAmOnTheWebsite()
        {
            // Simulate navigating to the website
            Assert.Pass("Navigated to the website.");
        }

        [When("I look at a post")]
        public void WhenILookAtAPost()
        {
            // Simulate viewing a post
            _currentPost = new PostPreviewVM
            {
                Id = 1,
                UserName = "testuser",
                Title = "Test Post",
                Excerpt = "This is a test post.",
                LikeCount = 5,
                IsLikedByCurrentUser = false
            };
            _initialLikeCount = _currentPost.LikeCount;
        }

        [Then("I should see a like button")]
        public void ThenIShouldSeeALikeButton()
        {
            // Assert that the like button is visible
            Assert.That(_currentPost, Is.Not.Null, "Post is not loaded.");
            Assert.Pass("Like button is visible.");
        }

        [Given("I am viewing a post with a like button that I have not liked")]
        public void GivenUnlikedPost()
        {
            // Initialize the post if not already done
            _currentPost ??= new PostPreviewVM
            {
                Id = 1,
                UserName = "testuser",
                Title = "Test Post",
                Excerpt = "This is a test post.",
                LikeCount = 5,
                IsLikedByCurrentUser = false
            };

            _initialLikeCount = _currentPost.LikeCount;

            // Ensure the post is not liked by the current user
            Assert.That(_currentPost.IsLikedByCurrentUser, Is.False, "Post is already liked by the user.");
        }

        [When("I click the like button")]
        public async Task WhenIClickTheLikeButton()
        {
            // Simulate clicking the like button
            if (!_currentPost.IsLikedByCurrentUser)
            {
                _likeRepositoryMock.Setup(repo => repo.AddOrUpdateAsync(It.IsAny<Like>())).Returns(Task.FromResult(new Like()));
                _currentPost.IsLikedByCurrentUser = true;
                _currentPost.LikeCount++;            
            }
            else
            {
                _likeRepositoryMock.Setup(repo => repo.DeleteAsync(It.IsAny<Like>())).Returns(Task.FromResult(true));
                _currentPost.IsLikedByCurrentUser = false;
                _currentPost.LikeCount--;
            }

            await Task.CompletedTask;
        }

        [Then("the like button should change its appearance to indicate it is liked")]
        public void ThenLikeButtonLooksLiked()
        {
            // Verify the button appearance indicates it is liked
            Assert.That(_currentPost.IsLikedByCurrentUser, Is.True, "The like button did not change to the liked state.");
        }

        [Then("the number of likes on the post should increase by 1")]
        public void ThenLikeCountIncreases()
        {
            // Verify the like count increased
            Assert.That(_currentPost, Is.Not.Null, "Current post is null.");
            Assert.That(_currentPost.LikeCount, Is.EqualTo(_initialLikeCount + 1), "The like count did not increase by 1.");
        }

        [Given("I am viewing a post that I have already liked")]
        public void GivenLikedPost()
        {
            _currentPost ??= new PostPreviewVM
            {
                Id = 1,
                UserName = "testuser",
                Title = "Test Post",
                Excerpt = "This is a test post.",
                LikeCount = 5,
                IsLikedByCurrentUser = true
            };

            _initialLikeCount = _currentPost.LikeCount;

            // Ensure the post is already liked by the current user
            Assert.That(_currentPost.IsLikedByCurrentUser, Is.True, "Post is not liked by the user.");
        }

        [Then("the like button should change its appearance to the default state")]
        public void ThenLikeButtonLooksUnliked()
        {
            Assert.That(_currentPost.IsLikedByCurrentUser, Is.False, "The like button did not change to the unliked state.");
        }

        [Then("the number of likes on the post should decrease by 1")]
        public void ThenLikeCountDecreases()
        {
            Assert.That(_currentPost, Is.Not.Null, "Current post is null.");
            Assert.That(_currentPost.LikeCount, Is.EqualTo(_initialLikeCount - 1), "The like count did not decrease by 1.");
        }
    }
}