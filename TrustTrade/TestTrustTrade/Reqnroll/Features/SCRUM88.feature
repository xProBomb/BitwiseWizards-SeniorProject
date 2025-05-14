Feature: Saving a post
    As a user
    I want to be able to add a post to my favorites
    So I can easily come back to it at a later time

    Background:
        Given I am logged in

    Scenario: Save button appears on a post
        Given I am on the page for a post
        When I look at the post
        Then the save button should be visible on the post

    Scenario: Saving a post
        Given I am on the page for a post
        And I see a save button on the post
        And the post has not been saved by me
        When I click the save button
        Then the post should be marked as saved

    Scenario: Unsaving a post
        Given I am on the page for a post
        And I see a save button
        And the post has been saved by me
        When I click the save button again
        Then the post should be marked as unsaved

    Scenario: Seeing saved posts
        Given I have saved a post
        When I go to my saved posts page
        Then I should see the saved post listed
        And I should be able to click on it to view it
