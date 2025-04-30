using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using TestTrustTrade.Reqnroll.Services;

namespace TestTrustTrade.Reqnroll.Services
{
    /// <summary>
    /// Service to handle login/logout functionality for tests
    /// </summary>
    public class LoginService
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        
        // Default test user credentials
        public static readonly TestUser User1 = new TestUser
        {
            Username = "testuser1",
            Email = "testuser1@example.com",
            Password = "Password123!"
        };
        
        public static readonly TestUser User2 = new TestUser
        {
            Username = "testuser2",
            Email = "testuser2@example.com",
            Password = "Password123!"
        };

        public static readonly TestUser AdminUser = new TestUser
        {
            Username = "admin",
            Email = "adminuser@example.com",
            Password = "AdminPassword123!",
            IsAdmin = true
        };

        public LoginService()
        {
            _driver = WebDriverSingleton.Instance;
            _wait = WebDriverSingleton.GetWait();
        }

    

        /// <summary>
        /// Logs in with the specified user credentials
        /// </summary>
        /// <param name="user">User credentials to use</param>
        /// <returns>True if login was successful</returns>
        public bool Login(TestUser user)
        {
            try
            {
                // Navigate to login page
                _driver.Navigate().GoToUrl($"{WebDriverSingleton.BaseUrl}/Identity/Account/Login");
                
                // Wait for login form to load
                _wait.Until(d => d.FindElement(By.Id("Input_Email")));
                
                // Clear fields and enter credentials
                var emailField = _driver.FindElement(By.Id("Input_Email"));
                emailField.Clear();
                emailField.SendKeys(user.Email);
                
                var passwordField = _driver.FindElement(By.Id("password-field"));
                passwordField.Clear();
                passwordField.SendKeys(user.Password);
                
                // Submit login form
                _driver.FindElement(By.TagName("form")).Submit();
                
                // Wait for redirect to home page
                _wait.Until(d => d.Url.Contains("/Home") || d.PageSource.Contains("Logout"));
                
                // Verify we're logged in
                return _driver.PageSource.Contains("Logout");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Logs out the current user
        /// </summary>
        /// <returns>True if logout was successful</returns>
        public bool Logout()
        {
            try
            {
                // Click on profile dropdown
                _driver.FindElement(By.Id("profileDropdown")).Click();
                
                // Wait briefly for animation
                System.Threading.Thread.Sleep(1000);
                
                // Wait for dropdown menu
                _wait.Until(d => d.FindElement(By.CssSelector("form[action*='Logout']")));
                
                // Click logout button
                _driver.FindElement(By.CssSelector("form[action*='Logout'] button")).Click();
                
                // Wait for redirect to home page or login page
                _wait.Until(d => d.Url.Contains("") || d.Url.Contains("/Login"));
                
                // Verify we're logged out
                return _driver.PageSource.Contains("Login") && !_driver.PageSource.Contains("Logout");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logout failed: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Model to hold test user credentials
    /// </summary>
    public class TestUser
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; } = false; // Default to false
    }
}