Feature: UserStory80-Market

  Scenario: Filtering stocks by partial ticker
    Given I have a list of stocks including "AAPL", "GOOGL", and "TSLA"
    When I filter the stocks with the query "AAP"
    Then the result should include "AAPL" only

  Scenario: Switching to crypto view
    Given I am on the market page with stocks and crypto
    When I select the "crypto" view
    Then I should see a list that includes "BTC" and not "AAPL"

  Scenario: Switching to stock view
    Given I am on the market page with stocks and crypto
    When I select the "stock" view
    Then I should see a list that includes "AAPL" and not "BTC"
