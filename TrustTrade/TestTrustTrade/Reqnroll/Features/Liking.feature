Feature: Liking a Post

  Scenario: Viewing a post shows a like button
    Given I am on the website
    When I look at a post
    Then I should see a like button

  Scenario: Liking a post I have not liked
    Given I am viewing a post with a like button that I have not liked
    When I click the like button
    Then the like button should change its appearance to indicate it is liked
    And the number of likes on the post should increase by 1

  Scenario: Unliking a post I have already liked
    Given I am viewing a post that I have already liked
    When I click the like button
    Then the like button should change its appearance to the default state
    And the number of likes on the post should decrease by 1
