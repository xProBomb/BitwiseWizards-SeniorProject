Feature: Profile Picture Upload with Cropper_js
  As a user
  I want to upload and crop my profile picture
  So that I can customize my profile appearance

  Scenario: Successfully uploading and cropping a profile picture
    Given I am on the Profile page
    When I click the "Change" button
    And I upload a valid image file
    And I crop the image using the cropper tool
    And I click the "Crop & Upload" button
    Then the profile picture should be updated successfully

  Scenario: Uploading an invalid file type
    Given I am on the Profile page
    When I click the "Change" button
    And I upload an invalid image file
    Then I should see an error message indicating an invalid file type