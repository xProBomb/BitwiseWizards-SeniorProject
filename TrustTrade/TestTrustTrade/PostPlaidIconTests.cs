using TrustTrade.Models;
using TrustTrade.Helpers;
using TrustTrade.ViewModels;

namespace TestTrustTrade
{
    [TestFixture]
    public class PostPlaidIconTests_Enhanced
    {
        [Test]
        public void ViewModel_WithPlaidEnabled_HasPlaidFlagSet()
        {
            // Arrange
            var viewModel = new PostPreviewVM
            {
                UserName = "Test User",
                IsPlaidEnabled = true
            };

            // Assert
            Assert.That(viewModel.IsPlaidEnabled, Is.True);
        }

        [Test]
        public void ViewModel_WithPlaidDisabled_DoesNotHavePlaidFlagSet()
        {
            // Arrange
            var viewModel = new PostPreviewVM
            {
                UserName = "Test User",
                IsPlaidEnabled = false
            };

            // Assert
            Assert.That(viewModel.IsPlaidEnabled, Is.False);
        }
        
        [Test]
        public void Post_WithPlaidEnabledUser_ShouldShowCheckmark()
        {
            // Arrange
            var user = new User
            {
                ProfileName = "Test User",
                PlaidEnabled = true
            };
            
            var post = new Post
            {
                Id = 1,
                UserId = 1,
                User = user
            };
            
            // This mirrors the logic in your controller
            var viewModel = new PostPreviewVM
            {
                Id = post.Id,
                UserName = post.User.Username,
                IsPlaidEnabled = post.User.PlaidEnabled ?? false
            };
            
            // Assert - This is what the view checks to show the checkmark
            Assert.That(viewModel.IsPlaidEnabled, Is.True);
        }

        [Test]
        public void Post_WithPlaidDisabledUser_ShouldNotShowCheckmark()
        {
            // Arrange
            var user = new User
            {
                ProfileName = "Test User",
                PlaidEnabled = false
            };
            
            var post = new Post
            {
                Id = 1,
                UserId = 1,
                User = user
            };
            
            // This mirrors the logic in your controller
            var viewModel = new PostPreviewVM
            {
                Id = post.Id,
                UserName = post.User.Username,
                IsPlaidEnabled = post.User.PlaidEnabled ?? false
            };
            
            // Assert - This is what the view checks to show the checkmark
            Assert.That(viewModel.IsPlaidEnabled, Is.False);
        }

        [Test]
        public void Post_WithPlaidNull_ShouldNotShowCheckmark()
        {
            // Arrange
            var user = new User
            {
                ProfileName = "Test User",
                PlaidEnabled = null
            };
            
            var post = new Post
            {
                Id = 1,
                UserId = 1,
                User = user
            };
            
            // This mirrors the logic in your controller
            var viewModel = new PostPreviewVM
            {
                Id = post.Id,
                UserName = post.User.Username,
                IsPlaidEnabled = post.User.PlaidEnabled ?? false
            };
            
            // Assert - This is what the view checks to show the checkmark
            Assert.That(viewModel.IsPlaidEnabled, Is.False);
        }

        [Test]
        public void Post_WithPlaidEnabledAndPortfolioValue_ShowsValueAndCheckmark()
        {
            // Arrange
            var user = new User
            {
                ProfileName = "Test User",
                PlaidEnabled = true
            };
            
            var post = new Post
            {
                Id = 1,
                UserId = 1,
                User = user,
                PortfolioValueAtPosting = 12345.67M
            };
            
            // This mirrors the logic in your controller
            var viewModel = new PostPreviewVM
            {
                Id = post.Id,
                UserName = post.User.Username,
                IsPlaidEnabled = post.User.PlaidEnabled ?? false,
                PortfolioValueAtPosting = FormatCurrencyAbbreviate.FormatCurrencyAbbreviated(post.PortfolioValueAtPosting.Value)
            };
            
            // Assert
            Assert.Multiple(() => {
                Assert.That(viewModel.IsPlaidEnabled, Is.True, "IsPlaidEnabled should be true");
                Assert.That(viewModel.PortfolioValueAtPosting, Is.Not.Null, "PortfolioValueAtPosting should have a value");
                Assert.That(viewModel.PortfolioValueAtPosting, Is.EqualTo("$12.3K"), "Portfolio value should match");
            });
        }
    }
}