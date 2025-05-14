Feature: Post Images
As a user
I want to add an image to my post

    Scenario: User can add image to post
        Given User1 is logged in
        When User1 navigates to post creation
        And User1 adds an image to post
        Then the post should display image

