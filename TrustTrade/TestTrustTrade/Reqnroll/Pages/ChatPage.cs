using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Threading;

namespace TestTrustTrade.Reqnroll.Pages
{
    /// <summary>
    /// Page object for chat-related pages
    /// </summary>
    public class ChatPage : BasePage
    {
        // Locators
        private readonly By _messageInput = By.Id("messageInput");
        private readonly By _sendButton = By.ClassName("bi-send");
        private readonly By _messagesList = By.Id("messagesList");
        private readonly By _messageContainers = By.CssSelector(".message-container");
        private readonly By _chatCards = By.CssSelector(".conversation-item");
        private readonly By _archiveButton = By.Id("archiveConversationBtn");
        private readonly By _messageButton = By.CssSelector("form[action*='StartConversation'] button");
        private readonly By _backToInboxButton = By.CssSelector("a[href*='Index']");
        private readonly By _otherUsername = By.CssSelector(".card-header h5");
        
        /// <summary>
        /// Gets the URL path for the chat page
        /// </summary>
        protected override string PageUrl => "/Chat";
        
        /// <summary>
        /// Checks if any chat page is loaded (inbox or conversation)
        /// </summary>
        public override bool IsPageLoaded()
        {
            try
            {
                return Driver.Url.Contains("/Chat") &&
                       (Driver.FindElements(By.CssSelector(".card-header")).Count > 0);
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Checks if the inbox page is loaded
        /// </summary>
        public bool IsInboxLoaded()
        {
            try
            {
                return Driver.Url.EndsWith("/Chat") || Driver.Url.EndsWith("/Chat/Index");
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Checks if a conversation page is loaded
        /// </summary>
        public bool IsConversationLoaded()
        {
            try
            {
                return Driver.Url.Contains("/Chat/Conversation/");
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Starts a chat with another user from their profile
        /// </summary>
        /// <returns>True if successful</returns>
        public bool StartChat()
        {
            try
            {
                // Check if we're on a profile page
                if (!Driver.Url.Contains("/Profile/User/"))
                {
                    return false;
                }
                
                // Find and click the message button
                var messageButton = WaitForElementClickable(_messageButton);
                if (messageButton == null)
                {
                    return false;
                }
                
                messageButton.Click();
                
                // Wait for redirect to conversation
                return WaitForConversationPageLoad();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start chat: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Gets the other user's username from the conversation
        /// </summary>
        /// <returns>The username or empty string if not found</returns>
        public string GetOtherUsername()
        {
            try
            {
                return Driver.FindElement(_otherUsername).Text;
            }
            catch
            {
                return string.Empty;
            }
        }
        
        /// <summary>
        /// Waits for the conversation page to load
        /// </summary>
        /// <param name="timeoutInSeconds">Timeout in seconds</param>
        /// <returns>True if loaded successfully</returns>
        private bool WaitForConversationPageLoad(int timeoutInSeconds = 10)
        {
            try
            {
                Wait.Until(d => d.Url.Contains("/Chat/Conversation/"));
                WaitForElementVisible(_messageInput);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Sends a message in the current conversation
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <returns>True if successful</returns>
        public bool SendMessage(string message)
        {
            try
            {
                if (!IsConversationLoaded())
                {
                    return false;
                }
                
                var inputField = WaitForElementVisible(_messageInput);
                inputField.Clear();
                inputField.SendKeys(message);
                
                var sendButton = WaitForElementClickable(_sendButton);
                sendButton.Click();
                
                // Wait for the message to appear in the chat
                Wait.Until(d => d.FindElements(_messageContainers)
                    .Count > 0 && d.FindElements(_messageContainers)
                    .Any(m => m.Text.Contains(message)));
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send message: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Checks if a specific message is visible in the conversation
        /// </summary>
        /// <param name="message">Message text to check for</param>
        /// <returns>True if the message is visible</returns>
        public bool IsMessageVisible(string message)
        {
            try
            {
                if (!IsConversationLoaded())
                {
                    return false;
                }
                
                return Driver.FindElements(_messageContainers)
                    .Any(m => m.Text.Contains(message));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to check if message is visible: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Gets all messages in the current conversation
        /// </summary>
        /// <returns>List of message texts</returns>
        public List<string> GetMessages()
        {
            try
            {
                if (!IsConversationLoaded())
                {
                    return new List<string>();
                }
                
                var messageElements = Driver.FindElements(_messageContainers);
                return messageElements.Select(m => m.Text).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get messages: {ex.Message}");
                return new List<string>();
            }
        }
        
        /// <summary>
        /// Archives the current conversation
        /// </summary>
        /// <returns>True if successful</returns>
        public bool ArchiveConversation()
        {
            try
            {
                if (!IsConversationLoaded())
                {
                    return false;
                }
                
                // Click the dropdown menu to show options
                Driver.FindElement(By.Id("chatOptionsDropdown")).Click();
                
                // Wait for dropdown to appear
                Thread.Sleep(500);
                
                // Click archive option
                var archiveButton = WaitForElementClickable(_archiveButton);
                archiveButton.Click();
                
                // Confirm the archive action
                IAlert alert = Wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.AlertIsPresent());
                alert.Accept();
                
                // Wait for redirect back to inbox
                Wait.Until(d => d.Url.EndsWith("/Chat") || d.Url.EndsWith("/Chat/Index"));
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to archive conversation: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Navigates back to the chat inbox
        /// </summary>
        /// <returns>True if successful</returns>
        public bool GoBackToInbox()
        {
            try
            {
                if (!IsConversationLoaded())
                {
                    return true; // Already on inbox
                }
                
                var backButton = WaitForElementClickable(_backToInboxButton);
                backButton.Click();
                
                // Wait for redirect to inbox
                Wait.Until(d => d.Url.EndsWith("/Chat") || d.Url.EndsWith("/Chat/Index"));
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to go back to inbox: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Opens a conversation with a specific user from the inbox
        /// </summary>
        /// <param name="username">Username to look for</param>
        /// <returns>True if successful</returns>
        public bool OpenConversationWithUser(string username)
        {
            try
            {
                if (!IsInboxLoaded())
                {
                    return false;
                }
                
                var conversations = Driver.FindElements(_chatCards);
                var targetConversation = conversations.FirstOrDefault(c => c.Text.Contains(username));
                
                if (targetConversation == null)
                {
                    return false;
                }
                
                targetConversation.Click();
                
                // Wait for redirect to conversation
                return WaitForConversationPageLoad();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open conversation with user: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Checks if a conversation with a specific user exists in the inbox
        /// </summary>
        /// <param name="username">Username to look for</param>
        /// <returns>True if the conversation exists</returns>
        public bool ConversationExists(string username)
        {
            try
            {
                if (!IsInboxLoaded())
                {
                    return false;
                }
                
                var conversations = Driver.FindElements(_chatCards);
                return conversations.Any(c => c.Text.Contains(username));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to check if conversation exists: {ex.Message}");
                return false;
            }
        }
    }
}