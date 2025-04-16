Feature: Contact Support Form
As a logged-in user
I want to submit a contact support request
So that I can get help with issues or provide feedback

    Background: User is Logged In
        Given the user "testuser@example.com" is logged in

    Scenario: Successfully submitting a support request
        Given the user navigates to the Contact Support page
        When the user enters "Test User Name" into the Name field
        And the user selects "Question" as the Category
        And the user enters "This is my support message asking a question." into the Message field
        And the user submits the support form
        Then an email should be sent to the support address "trusttrade.auth@gmail.com" with category "Question"
        And the user should be redirected back to the Contact Support page
        And a success message "Your request has been submitted. We will get back to you shortly." should be set