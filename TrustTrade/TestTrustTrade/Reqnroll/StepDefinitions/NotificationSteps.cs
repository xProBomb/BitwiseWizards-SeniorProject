using OpenQA.Selenium;
using Reqnroll;
using TestTrustTrade.Reqnroll.Pages;
using TestTrustTrade.Reqnroll.Services;

namespace TestTrustTrade.Reqnroll.StepDefinitions
{
    [Binding]
    public class NotificationSteps : IDisposable
    {
        private readonly LoginService _loginService;
        private readonly NotificationPage _notificationPage;
        private readonly ProfilePage _user1ProfilePage;
        private readonly ProfilePage _user2ProfilePage;
        
        // Store the current WebDriver reference for easier access
        private readonly IWebDriver _driver;

        // Remember user usernames for later steps
        private readonly string _user1Username;
        private readonly string _user2Username;

        public NotificationSteps()
        {
            // Get driver reference from the singleton
            _driver = WebDriverSingleton.Instance;
            
            // Initialize services and page objects
            _loginService = new LoginService();
            _notificationPage = new NotificationPage();
            
            // Get usernames for profile pages
            _user1Username = LoginService.User1.Username;
            _user2Username = LoginService.User2.Username;
            
            // Initialize profile pages
            _user1ProfilePage = new ProfilePage(_user1Username);
            _user2ProfilePage = new ProfilePage(_user2Username);
        }

        [Given(@"User1 is logged in")]
        public void GivenUser1IsLoggedIn()
        {
            // Use the login service to log in as User1
            bool success = _loginService.Login(LoginService.User1);
            
            // Verify we're logged in
            Assert.That(success, Is.True, "Failed to log in as User1");
        }

        [When(@"User1 follows User2")]
        public void WhenUser1FollowsUser2()
        {
            // Navigate to User2's profile
            _user2ProfilePage.NavigateTo();
            
            // Verify the profile loaded
            Assert.That(_user2ProfilePage.IsPageLoaded(), Is.True, "User2's profile page failed to load");
            
            // First unfollow if already following
            if (_user2ProfilePage.IsFollowing())
            {
                bool unfollowSuccess = _user2ProfilePage.UnfollowUser();
                Assert.That(unfollowSuccess, Is.True, "Failed to unfollow User2");
            }
            
            // Now follow
            bool followSuccess = _user2ProfilePage.FollowUser();
            Assert.That(followSuccess, Is.True, "Failed to follow User2");
        }

        [When(@"User1 logs out")]
        public void WhenUser1LogsOut()
        {
            // Use the login service to log out
            bool success = _loginService.Logout();
            
            // Verify we're logged out
            Assert.That(success, Is.True, "Failed to log out User1");
        }

        [When(@"User2 logs in")]
        public void WhenUser2LogsIn()
        {
            // Use the login service to log in as User2
            bool success = _loginService.Login(LoginService.User2);
            
            // Verify we're logged in
            Assert.That(success, Is.True, "Failed to log in as User2");
        }

        [Then(@"User2 should see a notification indicating User1 is following them")]
        public void ThenUser2ShouldSeeANotificationIndicatingUser1IsFollowingThem()
        {
            // Open notification dropdown
            _notificationPage.OpenNotificationDropdown();
            
            // Wait briefly for animation
            System.Threading.Thread.Sleep(500);
            
            // Check for notification text
            string expectedText = $"{_user1Username} started following you";
            bool notificationFound = _driver.PageSource.Contains(expectedText);
            
            // Verify notification exists
            Assert.That(notificationFound, Is.True, $"Notification '{expectedText}' not found");
        }

        [When(@"User2 clicks on the notification")]
        public void WhenUser2ClicksOnTheNotification()
        {
            // Navigate to notifications page
            _notificationPage.NavigateTo();
            Assert.That(_notificationPage.IsPageLoaded(), Is.True, "Notifications page failed to load");
            
            // Find and click on the notification
            string notificationText = $"{_user1Username} started following you";
            bool clickSuccess = _notificationPage.ClickNotificationByText(notificationText);
            
            // Verify notification was clicked
            Assert.That(clickSuccess, Is.True, $"Failed to click on notification containing '{notificationText}'");
        }

        [Then(@"User2 should be redirected to User1's profile")]
        public void ThenUser2ShouldBeRedirectedToUser1Profile()
        {
            // Verify we're on User1's profile page
            Assert.That(_user1ProfilePage.IsPageLoaded(), Is.True, "Failed to load User1's profile page");
            
            // Additional verification of the username
            string displayedUsername = _user1ProfilePage.GetDisplayedUsername();
            Assert.That(displayedUsername, Contains.Substring(_user1Username), 
                $"Expected username '{_user1Username}' but found '{displayedUsername}'");
        }

        [When(@"User2 returns to notifications page")]
        public void WhenUser2ReturnsToNotificationsPage()
        {
            // Navigate to notifications page
            _notificationPage.NavigateToHistory();
            
            // Verify page loaded
            Assert.That(_notificationPage.IsPageLoaded(), Is.True, "Notifications page failed to load");
        }

        [When(@"User2 archives the notification")]
        public void WhenUser2ArchivesTheNotification()
        {
            // Find and archive notification from User1
            string notificationText = _user1Username;
            Thread.Sleep(1000);
            bool archiveSuccess = _notificationPage.ArchiveNotification(notificationText);
            
            // Verify notification was archived
            Assert.That(archiveSuccess, Is.True, $"Failed to archive notification containing '{notificationText}'");
        }

        [Then(@"The notification should be removed from the list")]
        public void ThenTheNotificationShouldBeRemovedFromTheList()
        {
            // Check if notification still exists
            string notificationText = _user1Username;
            bool notificationExists = _notificationPage.NotificationExists(notificationText);
            
            // Verify notification was removed
            Assert.That(notificationExists, Is.False, $"Notification containing '{notificationText}' still exists but should have been removed");
        }

        public void Dispose()
        {
            // Release the driver instance when this step definition is disposed
            // but don't actually quit the driver since other tests might use it
            WebDriverSingleton.ReleaseInstance();
        }
        
        [AfterTestRun]
        public static void CleanUp()
        {
            // Force quit the driver after all tests are complete
            WebDriverSingleton.ForceQuit();
        }
    }
}