using OpenQA.Selenium;
using Reqnroll;
using TestTrustTrade.Reqnroll.Pages;
using TestTrustTrade.Reqnroll.Services;

namespace TestTrustTrade.Reqnroll.StepDefinitions
{
    [Binding]
    public class ChatStepDefinitions : IDisposable
    {
        private readonly LoginService _loginService;
        private readonly ChatPage _chatPage;
        private readonly ProfilePage _user1ProfilePage;
        private readonly ProfilePage _user2ProfilePage;
        private readonly NotificationPage _notificationPage;
        
        // Store the current WebDriver reference for easier access
        private readonly IWebDriver _driver;

        // Remember user usernames for later steps
        private readonly string _user1Username;
        private readonly string _user2Username;
        
        // Store messages for verification
        private string _firstMessage;
        private string _replyMessage;

        public ChatStepDefinitions()
        {
            // Get driver reference from the singleton
            _driver = WebDriverSingleton.Instance;
            
            // Initialize services and page objects
            _loginService = new LoginService();
            _chatPage = new ChatPage();
            _notificationPage = new NotificationPage();
            
            // Get usernames for profile pages
            _user1Username = LoginService.User1.Username;
            _user2Username = LoginService.User2.Username;
            
            // Initialize profile pages
            _user1ProfilePage = new ProfilePage(_user1Username);
            _user2ProfilePage = new ProfilePage(_user2Username);
        }

        [When(@"User1 navigates to User2's profile")]
        public void WhenUser1NavigatesToUser2Profile()
        {
            // Navigate to User2's profile
            _user2ProfilePage.NavigateTo();
            
            // Verify the profile loaded
            Assert.That(_user2ProfilePage.IsPageLoaded(), Is.True, "User2's profile page failed to load");
            
            // Verify we're on the right profile
            string displayedUsername = _user2ProfilePage.GetDisplayedUsername();
            Assert.That(displayedUsername, Contains.Substring(_user2Username), 
                $"Expected username '{_user2Username}' but found '{displayedUsername}'");
        }

        [When(@"User1 starts a chat with User2")]
        public void WhenUser1StartsAChatWithUser2()
        {
            // Start a chat from User2's profile
            bool success = _chatPage.StartChat();
            
            // Verify we're redirected to the chat page
            Assert.That(success, Is.True, "Failed to start a chat with User2");
            Assert.That(_chatPage.IsConversationLoaded(), Is.True, "Chat conversation page failed to load");
            
            // Verify we're chatting with the right user
            string otherUsername = _chatPage.GetOtherUsername();
            Assert.That(otherUsername, Contains.Substring(_user2Username), 
                $"Expected to chat with '{_user2Username}' but found '{otherUsername}'");
        }

        [When(@"User1 sends a message ""(.*)""")]
        public void WhenUser1SendsAMessage(string message)
        {
            // Store the message for later verification
            _firstMessage = message;
            
            // Send the message
            bool success = _chatPage.SendMessage(message);
            
            // Verify the message was sent
            Assert.That(success, Is.True, $"Failed to send message: {message}");
            
            // Verify the message appears in the conversation
            Assert.That(_chatPage.IsMessageVisible(message), Is.True, 
                $"Message '{message}' not visible in the conversation");
        }

        [Then(@"User2 should see a notification about the new message")]
        public void ThenUser2ShouldSeeANotificationAboutTheNewMessage()
        {
            // Open notification dropdown
            _notificationPage.OpenNotificationDropdown();
            
            // Wait briefly for animation
            Thread.Sleep(500);
            
            // Check for notification text
            string expectedText = $"{_user1Username} sent you a message";
            bool notificationFound = _driver.PageSource.Contains(expectedText);
            
            // Verify notification exists
            Assert.That(notificationFound, Is.True, $"Notification '{expectedText}' not found");
        }

        [When(@"User2 clicks on the notification for chat")]
        public void WhenUser2ClicksOnTheNotification()
        {
            // Navigate to notifications page
            _notificationPage.NavigateTo();
            Assert.That(_notificationPage.IsPageLoaded(), Is.True, "Notifications page failed to load");
            
            // Find and click on the notification
            string notificationText = $"{_user1Username} sent you a message";
            bool clickSuccess = _notificationPage.ClickNotificationByText(notificationText);
            
            // Verify notification was clicked
            Assert.That(clickSuccess, Is.True, $"Failed to click on notification containing '{notificationText}'");
        }

        [Then(@"User2 should be redirected to the conversation with User1")]
        public void ThenUser2ShouldBeRedirectedToTheConversationWithUser1()
        {
            // Verify we're on the chat conversation page
            Assert.That(_chatPage.IsConversationLoaded(), Is.True, "Chat conversation page failed to load");
            
            // Verify we're chatting with the right user
            string otherUsername = _chatPage.GetOtherUsername();
            Assert.That(otherUsername, Contains.Substring(_user1Username), 
                $"Expected to chat with '{_user1Username}' but found '{otherUsername}'");
        }

        [Then(@"User2 should see the message from User1")]
        public void ThenUser2ShouldSeeTheMessageFromUser1()
        {
            // Verify the message is visible
            Assert.That(_chatPage.IsMessageVisible(_firstMessage), Is.True, 
                $"Message '{_firstMessage}' not visible in the conversation");
        }

        [When(@"User2 sends a reply ""(.*)""")]
        public void WhenUser2SendsAReply(string message)
        {
            // Store the reply for later verification
            _replyMessage = message;
            
            // Send the reply
            bool success = _chatPage.SendMessage(message);
            
            // Verify the message was sent
            Assert.That(success, Is.True, $"Failed to send reply: {message}");
        }

        [Then(@"The message should appear in the conversation")]
        public void ThenTheMessageShouldAppearInTheConversation()
        {
            // Verify the reply appears in the conversation
            Assert.That(_chatPage.IsMessageVisible(_replyMessage), Is.True, 
                $"Reply '{_replyMessage}' not visible in the conversation");
            
            // Verify both messages are visible
            var messages = _chatPage.GetMessages();
            Assert.That(messages.Any(m => m.Contains(_firstMessage)), Is.True, 
                $"First message '{_firstMessage}' is not in the conversation");
            Assert.That(messages.Any(m => m.Contains(_replyMessage)), Is.True, 
                $"Reply '{_replyMessage}' is not in the conversation");
        }

        [When(@"User2 archives the conversation")]
        public void WhenUser2ArchivesTheConversation()
        {
            // Archive the conversation
            bool success = _chatPage.ArchiveConversation();
            
            // Verify we're redirected to the inbox
            Assert.That(success, Is.True, "Failed to archive conversation");
            Assert.That(_chatPage.IsInboxLoaded(), Is.True, "Chat inbox page failed to load after archiving");
            
            // Give the page time to fully load
            Thread.Sleep(1000);
        }

        [Then(@"The conversation should be removed from the list")]
        public void ThenTheConversationShouldBeRemovedFromTheList()
        {
            // Verify the conversation is no longer in the inbox
            bool conversationExists = _chatPage.ConversationExists(_user1Username);
            
            // Verify the conversation was removed
            Assert.That(conversationExists, Is.False, 
                $"Conversation with '{_user1Username}' still exists but should have been removed");
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