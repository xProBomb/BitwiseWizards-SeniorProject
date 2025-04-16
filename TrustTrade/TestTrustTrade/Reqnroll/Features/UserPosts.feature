Feature: User Posts

    Scenario: Clicking on a post redirect to the post details page
    Given I am viewing the list of a person's posts
    When I click on a post
    Then I should be redirected to the post details page
