Feature: Liking a comment
    As a user
    I want to be bale to like a comment
    So that I can show my appreciation for what others have to say

    Scenario: Like button appears on a comment
        Given I am on the page for a post
        And I see a comment
        Then the like button should be visible on the comment

    Scenario: Liking a comment
        Given I am logged in
        And I am on the page for a post
        And I see a comment
        And the comment has a like button
        And the comment has not been liked by me
        When I click the like button on the comment
        Then the comment should be marked as liked

    Scenario: Unliking a comment
        Given I am logged in
        And I am on the page for a post
        And I see a comment
        And the comment has been liked by me
        When I click the like button on the comment again
        Then the comment should be marked as unliked
