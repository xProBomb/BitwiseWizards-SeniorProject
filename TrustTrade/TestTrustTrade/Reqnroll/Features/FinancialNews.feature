Feature: Financial News
    As a user of TrustTrade
    I want to view financial news
    So that I can stay informed about market developments

Scenario: View all financial news
    Given I have a financial news controller
    And there are 10 news items in the database
    When I request the financial news index page
    Then the result should be a view
    And the view model should contain 10 news items

Scenario: View financial news with category filter
    Given I have a financial news controller
    And there are 10 news items in the database
    And I specify the category "Stock"
    When I request the financial news index page
    Then the result should be a view
    And the view model should contain 5 news items
    And the category in ViewBag should be "Stock"

Scenario: View financial news widget
    Given I have a financial news controller
    And there are 10 news items in the database
    And I want to see 5 items
    When I request the financial news widget
    Then the result should be a partial view
    And the view name should be "_NewsWidget"
    And the view model should contain 5 news items

Scenario: View financial news widget with category filter
    Given I have a financial news controller
    And there are 10 news items in the database
    And I specify the category "Crypto"
    And I want to see 3 items
    When I request the financial news widget
    Then the result should be a partial view
    And the view name should be "_NewsWidget"
    And the view model should contain 3 news items
    And the category in ViewBag should be "Crypto"

Scenario: Handle error in index page
    Given I have a financial news controller
    When the service throws an exception
    And I request the financial news index page
    Then the result should be an error view

Scenario: Handle error in widget
    Given I have a financial news controller
    When the service throws an exception
    And I request the financial news widget
    Then the result should be an error message