Feature: Market Modal History Filters
  As a user on the market page
  I want to view stock modal details
  And be able to select different history filter ranges (3D, 5D, 7D)
  So that I can analyze short-term stock performance trends

  Background:
    Given I am on the market page with modal

  Scenario: Modal opens on stock card click
    When I click on a stock card
    Then the stock modal should be visible

  Scenario: History filter buttons are visible
    When I click on a stock card
    Then the stock modal should be visible
    Then I should see history filter options

  Scenario: Click 3D history filter and see it active
    When I click on a stock card
    Then the stock modal should be visible
    When I click the '3D' history filter
    Then the '3D' history filter should be active

  Scenario: Click 5D history filter and see it active
    When I click on a stock card
    Then the stock modal should be visible
    When I click the '5D' history filter
    Then the '5D' history filter should be active

  Scenario: Click 7D history filter and see it active
    When I click on a stock card
    Then the stock modal should be visible
    When I click the '7D' history filter
    Then the '7D' history filter should be active
