Feature: Search functionality
  As a user
  I want to search for users and posts
  So that I can discover content and people on TrustTrade

  Scenario: Search for users and view a profile
    Given I am on the search page
    When I select "users" from the dropdown
    And I type "rowan" into the search bar
    And I wait for results to appear
    Then I should see user cards in the results
    When I click the "View Profile" button for the first result
    Then I should be taken to the user's profile page

  Scenario: Search for posts
    Given I am on the search page
    When I select "posts" from the dropdown
    And I type "bitcoin" into the search bar
    And I wait for results to appear
    Then I should see post cards in the results
