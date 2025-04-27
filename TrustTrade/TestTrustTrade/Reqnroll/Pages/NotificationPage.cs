using OpenQA.Selenium;

namespace TestTrustTrade.Reqnroll.Pages
{
    /// <summary>
    /// Page object for the Notifications page
    /// </summary>
    public class NotificationPage : BasePage
    {
        // Locators
        private readonly By _notificationList = By.CssSelector(".notification-list");
        private readonly By _notificationItems = By.CssSelector(".notification-item");
        private readonly By _notificationDropdown = By.Id("notificationDropdown");
        private readonly By _notificationsContent = By.ClassName("notification-content");
        private readonly By _markAllAsReadBtn = By.Id("markAllAsReadBtn");
        
        /// <summary>
        /// Gets the URL path for the notifications page
        /// </summary>
        protected override string PageUrl => "/Notifications";
        
        /// <summary>
        /// Checks if the notifications page is loaded
        /// </summary>
        /// <returns>True if the page is loaded</returns>
        public override bool IsPageLoaded()
        {
            try
            {
                return WaitForElementVisible(_notificationDropdown) != null || 
                       Driver.PageSource.Contains("You don't have any notifications yet");
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Opens the notification dropdown
        /// </summary>
        public void OpenNotificationDropdown()
        {
            SafeClick(_notificationDropdown);
            // Wait briefly for animation
            Thread.Sleep(500);

            WaitForElementVisible(_notificationsContent);
        }
        
        /// <summary>
        /// Gets all notifications from the page
        /// </summary>
        /// <returns>List of notification WebElements</returns>
        public IList<IWebElement> GetAllNotifications()
        {
            try
            {
                return Driver.FindElements(_notificationItems);
            }
            catch
            {
                return new List<IWebElement>();
            }
        }
        
        /// <summary>
        /// Finds a notification containing the specified text
        /// </summary>
        /// <param name="text">Text to search for</param>
        /// <returns>The notification element, or null if not found</returns>
        public IWebElement FindNotificationByText(string text)
        {
            var notifications = GetAllNotifications();
            return notifications.FirstOrDefault(n => n.Text.Contains(text));
        }
        
        /// <summary>
        /// Clicks on a notification containing the specified text
        /// </summary>
        /// <param name="text">Text to search for</param>
        /// <returns>True if the notification was found and clicked</returns>
        public bool ClickNotificationByText(string text)
        {
            var notification = FindNotificationByText(text);
            if (notification == null)
                return false;
            
            try
            {
                // Click on the notification content link inside the notification
                var link = notification.FindElement(By.CssSelector(".notification-content"));
                link.Click();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to click notification: {ex.Message}");
                return false;
            }
        }
        
        /*/// <summary>
        /// Marks a notification as read by clicking the mark-read button
        /// </summary>
        /// <param name="text">Text contained in the notification</param>
        /// <returns>True if the notification was found and marked as read</returns>
        public bool MarkNotificationAsRead(string text)
        {
            var notification = FindNotificationByText(text);
            if (notification == null)
                return false;
            
            try
            {
                var markReadButton = notification.FindElement(By.CssSelector(".mark-read-btn"));
                markReadButton.Click();
                
                // Wait for animation to complete
                Thread.Sleep(1000);
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to mark notification as read: {ex.Message}");
                return false;
            }
        }*/
        
        /// <summary>
        /// Archives a notification
        /// </summary>
        /// <param name="text">Text contained in the notification</param>
        /// <returns>True if the notification was found and archived</returns>
        public bool ArchiveNotification(string text)
        {
            var notification = FindNotificationByText(text);
            if (notification == null)
                return false;
            
            try
            {
                var archiveButton = notification.FindElement(By.CssSelector(".archive-btn"));
                archiveButton.Click();
                
                // Wait for animation to complete
                Thread.Sleep(1000);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to archive notification: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Navigates to the notifications history page
        /// </summary>
        public void NavigateToHistory()
        {
            Driver.Navigate().GoToUrl(BaseUrl + "/History");
            // Wait for page to load
            WaitForElementVisible(_notificationList);
        }

        
        /// <summary>
        /// Marks all notifications as read
        /// </summary>
        /// <returns>True if successful</returns>
        public bool MarkAllAsRead()
        {
            try
            {
                SafeClick(_markAllAsReadBtn);
                
                // Wait for animations to complete
                Thread.Sleep(1000);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to mark all notifications as read: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Checks if a notification with the specified text exists
        /// </summary>
        /// <param name="text">Text to search for</param>
        /// <returns>True if the notification exists</returns>
        public bool NotificationExists(string text)
        {
            return FindNotificationByText(text) != null;
        }
    }
}