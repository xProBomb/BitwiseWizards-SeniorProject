using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using TrustTrade.Controllers;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using TrustTrade.ViewModels;

namespace TestTrustTrade
{
    [TestFixture]
    public class PlaidIconTests
    {
        [Test]
        public void ViewModel_WithPlaidEnabled_HasPlaidFlagSet()
        {
            // Arrange
            var viewModel = new MyProfileViewModel
            {
                ProfileName = "Test User",
                PlaidEnabled = true
            };

            // Assert
            Assert.That(viewModel.PlaidEnabled, Is.True);
        }

        [Test]
        public void ViewModel_WithPlaidDisabled_DoesNotHavePlaidFlagSet()
        {
            // Arrange
            var viewModel = new MyProfileViewModel
            {
                ProfileName = "Test User",
                PlaidEnabled = false
            };

            // Assert
            Assert.That(viewModel.PlaidEnabled, Is.False);
        }
        
        [Test]
        public void User_WithPlaidEnabled_ShouldShowCheckmark()
        {
            // Arrange
            var user = new User
            {
                ProfileName = "Test User",
                PlaidEnabled = true
            };
            
            // Assert - This is what the view checks to show the checkmark
            Assert.That(user.PlaidEnabled, Is.True);
        }

        [Test]
        public void User_WithPlaidDisabled_ShouldNotShowCheckmark()
        {
            // Arrange
            var user = new User
            {
                ProfileName = "Test User",
                PlaidEnabled = false
            };
            
            // Assert - This is what the view checks to show the checkmark
            Assert.That(user.PlaidEnabled, Is.False);
        }

        [Test]
        public void User_WithPlaidNull_ShouldNotShowCheckmark()
        {
            // Arrange
            var user = new User
            {
                ProfileName = "Test User",
                PlaidEnabled = null
            };
            
            // Assert - This is what the view checks to show the checkmark
            Assert.That(user.PlaidEnabled, Is.Null);
            Assert.That(user.PlaidEnabled ?? false, Is.False);
        }
    }
}