Feature: Market Modal Expansion
  As a user on the market page
  I want to click a stock card
  So that the modal with stock details appears

  Scenario: Expanding stock modal
    Given I am on the market page with modal
    When I click on a stock card
    Then the stock modal should be visible
