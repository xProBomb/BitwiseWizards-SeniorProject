Feature: Error Page functionality
    As a user
    I want to see an informative error page when accessing invalid URLs
    So that I understand what went wrong and can navigate elsewhere

    Scenario: User is redirected to error page when accessing invalid URL
        Given I am using the TrustTrade application
        When I navigate to an invalid URL "/NonExistentPath"
        Then I should be redirected to the error page
        And I should see the error code "404"
        And I should see the text "Investment Not Found"
        And I should see a link to contact support
        And I should see a home button

    Scenario: User can return to home page from error page
        Given I am on the error page
        When I click the "Back to Home" button
        Then I should be redirected to the home page
        
    Scenario: User can contact support from error page
        Given I am on the error page
        When I see the "Contact Support" button
        Then I should verify that the contact support button exists on the error page