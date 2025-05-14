Feature: Leave Landing Page
As a user
I want to leave the landing page
So that I can navigate to other pages

    Scenario: Leave Landing Page
        Given I am on the landing page
        When I leave the landing page
        Then I should be taken to the Index page
        