Feature: Site is alive and healthy

Scenario: Root loads OK
    When user launches site
    Then page loaded ok
    And save a screenshot named 00_Root

Scenario Outline: Page navigates correctly from root
    When user navigates to <Page> page via NavMenu
    Then page title is <Title>
    And element h1 is <Heading>
    Then save a screenshot named <Id>_<Page>

Examples:
| Id | Page    | Title              | Heading           |
| 10 | Home    | Index              | Hello, world!     |
| 20 | Counter | Counter            | Counter           |
| 30 | Fetch   | Weather forecast   | Weather forecast  |

Scenario: Counter starts at zero
    When user navigates to Counter page via NavMenu
    Then currentCount is 0

Scenario: Counter increments when clicking button
    Given user navigated to Counter page via NavMenu
    When clicking Increment 5 times
    Then currentCount is 5
    And save a screenshot named 21_Counter_Incremented

Scenario: Weather forecasts load
    When user navigates to Fetch page via NavMenu
    Then a Results table is returned
    And it has 4 columns and 5 rows
