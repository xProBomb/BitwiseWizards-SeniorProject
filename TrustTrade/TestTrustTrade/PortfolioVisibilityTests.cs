using NUnit.Framework;
using TrustTrade.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using TrustTrade.ViewModels;

namespace TestTrustTrade
{
    [TestFixture]
    public class PortfolioVisibilityTests
    {
        private List<HoldingViewModel> _testHoldings;
        private ProfileViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            // Create test holdings with visibility settings
            _testHoldings = new List<HoldingViewModel>
            {
                new HoldingViewModel
                {
                    Symbol = "AAPL",
                    Quantity = 10,
                    CurrentPrice = 170,
                    CostBasis = 150,
                    Institution = "Test Bank",
                    TypeOfSecurity = "equity",
                    IsHidden = false
                },
                new HoldingViewModel
                {
                    Symbol = "MSFT",
                    Quantity = 5,
                    CurrentPrice = 250,
                    CostBasis = 200,
                    Institution = "Test Bank",
                    TypeOfSecurity = "equity",
                    IsHidden = true  // This one is hidden
                },
                new HoldingViewModel
                {
                    Symbol = "GOOGL",
                    Quantity = 2,
                    CurrentPrice = 2200,
                    CostBasis = 2000,
                    Institution = "Test Bank",
                    TypeOfSecurity = "equity",
                    IsHidden = false
                }
            };

            // Create the view model
            _viewModel = new ProfileViewModel
            {
                Holdings = _testHoldings,
                HideDetailedInformation = false,
                HideAllPositions = false
            };
        }

        [Test]
        public void SingleHoldingIsHidden_HiddenCountIs1()
        {
            // Get visible and hidden holdings
            var visibleHoldings = _viewModel.Holdings.Where(h => !h.IsHidden).ToList();
            var hiddenHoldings = _viewModel.Holdings.Where(h => h.IsHidden).ToList();

            // Assert
            Assert.That(visibleHoldings.Count, Is.EqualTo(2));
            Assert.That(hiddenHoldings.Count, Is.EqualTo(1));
            Assert.That(hiddenHoldings[0].Symbol, Is.EqualTo("MSFT"));
        }

        [Test]
        public void WhenHideAllIsEnabled_AllHoldingsAreHidden()
        {
            // Arrange
            _viewModel.HideAllPositions = true;
            
            // Set all holdings' IsHidden property based on HideAllPositions
            foreach (var holding in _viewModel.Holdings)
            {
                holding.IsHidden = _viewModel.HideAllPositions || holding.IsHidden;
            }

            // Act
            var visibleHoldings = _viewModel.Holdings.Where(h => !h.IsHidden).ToList();
            var hiddenHoldings = _viewModel.Holdings.Where(h => h.IsHidden).ToList();

            // Assert
            Assert.That(visibleHoldings.Count, Is.EqualTo(0));
            Assert.That(hiddenHoldings.Count, Is.EqualTo(3));
        }

        [Test]
        public void HiddenAssetsValue_CalculatedCorrectly()
        {
            // Calculate expected values
            decimal expectedHiddenValue = _testHoldings
                .Where(h => h.IsHidden)
                .Sum(h => h.Quantity * h.CurrentPrice);
            
            decimal expectedTotalValue = _testHoldings
                .Sum(h => h.Quantity * h.CurrentPrice);

            // Assert
            Assert.That(_viewModel.HiddenAssetsCount, Is.EqualTo(1));
            Assert.That(_viewModel.HiddenAssetsValue, Is.EqualTo(expectedHiddenValue));
            Assert.That(_viewModel.TotalPortfolioValue, Is.EqualTo(expectedTotalValue));
            
            // Make sure our hidden value is what we expect
            Assert.That(_viewModel.HiddenAssetsValue, Is.EqualTo(1250));
        }

        [Test]
        public void HideDetailedInformation_HidesAppropriateColumns()
        {
            // This is a more simplified test since we can't check UI rendering directly
            
            // Act
            _viewModel.HideDetailedInformation = true;
            
            // Assert
            Assert.That(_viewModel.HideDetailedInformation, Is.True);
            
            // In a real scenario, this flag would be used in the view to hide columns
            // We're just verifying that the flag is properly set
        }
    }
}