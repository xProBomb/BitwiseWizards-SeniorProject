Feature: Profile Background Image
As a user
I want to add a background image to my profile
So that I can customize my profile's appearance

    Scenario: User can add and remove background image
        Given User1 is logged in
        When User1 navigates to their profile
        And User1 adds a space cat background image
        Then the profile should display the background image
        When User1 removes the background image
        Then the profile should not display any background image