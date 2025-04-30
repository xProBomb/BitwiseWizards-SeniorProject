Feature: Chat functionality
As a user
I want to be able to chat with other users
So that I can discuss investment strategies and build my network

    Scenario: Users can exchange messages and manage conversations
        Given User1 is logged in
        When User1 navigates to User2's profile
        And User1 starts a chat with User2
        And User1 sends a message "Hello, let's discuss investment strategies!"
        And User1 logs out
        And User2 logs in
        Then User2 should see a notification about the new message
        When User2 clicks on the notification for chat
        Then User2 should be redirected to the conversation with User1
        And User2 should see the message from User1
        When User2 sends a reply "Sure, what are your thoughts on ETFs?"
        Then The message should appear in the conversation
        When User2 archives the conversation
        Then The conversation should be removed from the list