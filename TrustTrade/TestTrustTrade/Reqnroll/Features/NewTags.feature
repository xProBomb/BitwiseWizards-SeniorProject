Feature: Adding New Tags to Posts

    Scenario: Viewing tag section shows existing tags and add tag input
    Given I am creating a new post
    When I look at the tags section
    Then I should see existing tags as checkboxes
    And I should see an input field for adding new tags

    Scenario: Adding a new tag with valid input
    Given I am on the post creation page
    When I enter a new tag name in the input field
    And I click the "Add Tag" button
    Then the new tag should be added to the list as a selected checkbox