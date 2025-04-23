Feature: Routing for Home and Following Pages

  This feature tests the routing logic for the Home and Following pages, 
  ensuring that the `isFollowing` parameter is handled correctly.

  Scenario: Navigating to the Home page
    Given I am on the Following page
    When I click the "Home" link
    Then I should be redirected to the Home page

  Scenario: Navigating to the Following page
    Given I am on the Home page
    When I click the "Following" link
    Then I should be redirected to the Following page
