using OpenQA.Selenium;


namespace TestTrustTrade.Reqnroll.Pages
{
    /// <summary>
    /// Page object for user profile pages
    /// </summary>
    public class ProfilePage : BasePage
    {
        private readonly string _username;

        // Locators
        private readonly By _profileContainer = By.Id("profile-container");
        private readonly By _followButton = By.Id("followButton");
        private readonly By _unfollowButton = By.Id("followButton"); // Same ID, different text
        private readonly By _usernameLocator = By.Id("profile-username");
        
        /// <summary>
        /// Constructs a profile page for a specific user
        /// </summary>
        /// <param name="username">Username for the profile</param>
        public ProfilePage(string username) : base()
        {
            _username = username;
        }
        
        /// <summary>
        /// Gets the URL path for this profile page
        /// </summary>
        protected override string PageUrl => $"/Profile/User/{_username}";
        
        /// <summary>
        /// Checks if the profile page is loaded
        /// </summary>
        /// <returns>True if the page is loaded</returns>
        public override bool IsPageLoaded()
        {
            try
            {
                var container = WaitForElementVisible(_profileContainer);
                return container != null && Driver.Url.Contains($"/Profile/User/");
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Gets the displayed username from the profile page
        /// </summary>
        /// <returns>The displayed username</returns>
        public string GetDisplayedUsername()
        {
            try
            {
                return Driver.FindElement(_usernameLocator).Text;
            }
            catch
            {
                return string.Empty;
            }
        }
        
        /// <summary>
        /// Checks if the current user is following this profile
        /// </summary>
        /// <returns>True if following, false if not following or can't determine</returns>
        public bool IsFollowing()
        {
            try
            {
                var button = Driver.FindElement(_followButton);
                return button.Text.Contains("Unfollow");
            }
            catch
            {
                return false;
            }
        }
        
        
        /// <summary>
        /// Follows the user whose profile is being viewed
        /// </summary>
        /// <returns>True if successful</returns>
        public bool FollowUser()
        {
            try
            {
                // Wait for the button to be in the "Follow" state (not "Unfollow")
                Wait.Until(d => {
                    try {
                        var element = d.FindElement(_followButton);
                        var text = element.Text;
                        return text.Contains("Follow") && !text.Contains("Unfollow");
                    } catch (StaleElementReferenceException) {
                        return false; // Element is stale, wait for next retry
                    }
                });
        
                // Find the button again after waiting to avoid stale references
                var button = WaitForElementClickable(_followButton);
        
                // Check current state (defensive check)
                if (button.Text.Contains("Unfollow"))
                {
                    Console.WriteLine("Already following this user");
                    return true;
                }
        
                // Click the button with retry mechanism
                SafeClick(_followButton);
        
                // Wait for button text to change to "Unfollow"
                Wait.Until(d => {
                    try {
                        return d.FindElement(_followButton).Text.Contains("Unfollow");
                    } catch (StaleElementReferenceException) {
                        return false; // Element is stale, wait for next retry
                    }
                });
        
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to follow user: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Unfollows the user whose profile is being viewed
        /// </summary>
        /// <returns>True if successful</returns>
        public bool UnfollowUser()
        {
            try
            {
                // Wait for button text to be "Unfollow"
                Wait.Until(d => {
                    try {
                        return d.FindElement(_followButton).Text.Contains("Unfollow");
                    } catch (StaleElementReferenceException) {
                        return false; // Element is stale, wait for next retry
                    }
                });
        
                // Find the button again after waiting to avoid stale references
                var button = WaitForElementClickable(_unfollowButton);
        
                // Check current state (defensive check)
                if (button.Text.Contains("Follow") && !button.Text.Contains("Unfollow"))
                {
                    Console.WriteLine("Already not following this user");
                    return true;
                }
        
                // Click the button with retry mechanism
                SafeClick(_unfollowButton);
        
                // Wait for button text to change to "Follow"
                Wait.Until(d => {
                    try {
                        var text = d.FindElement(_unfollowButton).Text;
                        return text.Contains("Follow") && !text.Contains("Unfollow");
                    } catch (StaleElementReferenceException) {
                        return false; // Element is stale, wait for next retry
                    }
                });
        
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to unfollow user: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Ensures the user is being followed (follows if not already following)
        /// </summary>
        /// <returns>True if successful</returns>
        public bool EnsureFollowing()
        {
            if (!IsFollowing())
            {
                return FollowUser();
            }
            return true;
        }
        
        /// <summary>
        /// Ensures the user is not being followed (unfollows if currently following)
        /// </summary>
        /// <returns>True if successful</returns>
        public bool EnsureNotFollowing()
        {
            if (IsFollowing())
            {
                return UnfollowUser();
            }
            return true;
        }
    }
}