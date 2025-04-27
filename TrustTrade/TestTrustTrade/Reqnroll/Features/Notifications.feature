Feature: Notifications functionality
    As a user
    I want to receive and manage notifications
    So that I can stay informed about activities related to my account

    Scenario: User receives and views a follow notification
        Given User1 is logged in
        When User1 follows User2
        And User1 logs out
        And User2 logs in
        Then User2 should see a notification indicating User1 is following them
        When User2 clicks on the notification
        Then User2 should be redirected to User1's profile
        When User2 returns to notifications page
        And User2 archives the notification
        Then The notification should be removed from the list