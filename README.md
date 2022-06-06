# Blazor Functional Test Stack

Demonstrates a simple yet powerful approach to Behaviour-Driven Development
and Functional Testing in .NET on a Blazor app.

The stack is: [.NET](https://dotnet.microsoft.com/en-us/download) | [NUnit](https://nunit.org/) | [Playwright for .NET](https://playwright.dev/dotnet/docs/intro) | [TickSpec](https://github.com/fsprojects/TickSpec) | [FsUnit](https://fsprojects.github.io/FsUnit/)

I have found this stack enables me to really quickly add functional tests to a new
web app, using this process:

1. Copy entire Tests.Functional directory into an app
2. Change the local.runsettings to match default project port
3. Add data-test-id's to code under test
4. Change the .feature file to match the app

## How to try it

### Clone it

```
PS> git clone https://github.com/jcoliz/BlazorFunctionalTestStack.git
PS> cd BlazorFunctionalTestStack
```

### Build it

If you don't already have the .NET 6.0 SDK installed, be sure to get a copy first from the [Download .NET](https://dotnet.microsoft.com/en-us/download) page.

```
PS> dotnet build
```

### Install browsers

If this is your first time running the version of PlayWright used by the tests, you'll need to
install the browsers.

```
PS> pwsh .\Tests.Functional\bin\Debug\net6.0\playwright.ps1 install
```

### Run app in backround

This script requires PowerShell 7. If you are running an old version, this is a great time
to [upgrade](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows)! Otherwise, you could open another window and run it there.

```
PS> .\startbg.ps1

Id     Name            PSJobTypeName   State         HasMoreData     Location             Command
--     ----            -------------   -----         -----------     --------             -------
27     uitestsbg       BackgroundJob   Running       True            localhost            dotnet run
```

### Run the tests

```
PS> dotnet test

Test run for .\Tests.Functional\bin\Debug\net6.0\Tests.Functional.dll (.NETCoreApp,Version=v6.0)
Microsoft (R) Test Execution Command Line Tool Version 17.1.0
Copyright (c) Microsoft Corporation.  All rights reserved.

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7, Duration: 13 s - Tests.Functional.dll (net6.0)
```

### Stop the background app

Best to do it now before you forget!

```
PS> .\stopbg.ps1
```

### Examine the screen shots

```
PS> start .\Tests.Functional\bin\Debug\net6.0\Screenshot\
```

![Screenshots](/docs/images/Screenshots.png)

## Check out the tests

The tests are written in Gherkin. You can find the full set in the [Porfolio.feature](Tests.Functional/Portfolio.feature) file. Gherkin is a great way to write clear, expressive
tests that humans can make sense of.

Bonus is that you can write the Gherkin *before* writing any new code, to follow 
Behaviour Driven Development principles.

```Gherkin
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

Scenario: Counter increments when clicking button
    Given user navigated to Counter page via NavMenu
    When clicking Increment 5 times
    Then currentCount is 5
```

Then, each step is backed by a few lines of Playwright code. You may notice that the tests
are in F#. This is because TickSpec uses the language. Not to fear! F# is pretty easy, and generally more concise than the C# alternatives.

```F#
let [<Given>] ``user launched site`` (page: IPage) (uri: Uri) = 
    page.GotoAsync(uri.ToString()) |> Async.AwaitTask |> Async.RunSynchronously
```

```F#
let [<Then>] ``page loaded ok`` (response: IResponse) =
    response.Ok 
        |> should be True
```

```F#
let [<Then>] ``(\S*) is (.*)`` (element:string) (expected:string) (page: IPage) =
    page.TextContentAsync($"data-test-id={element}") 
        |> Async.AwaitTask 
        |> Async.RunSynchronously 
        |> should equal expected
```

## Using data-test-id selectors

By convention, I prefer to [define explicit contracts](https://playwright.dev/dotnet/docs/selectors#define-explicit-contract) for elements under test. This ensures that later if the text is changed, or the composition of the page is changed, it's highly likely that the tests will still pass.

Thus, the steps defined here use data-test-id by default.

```html
<p role="status">Current count: <span data-test-id="currentCount">@currentCount</span></p>

<button data-test-id="Increment" class="btn btn-primary" @onclick="IncrementCount">Click me</button>
```

## In-depth look at the stack

### .NET

The first choice is to write the tests in the same framework used to write the code.
Personally, I prefer .NET for everything, so it's my default starting point.

### NUnit

I actually prefer MSTest for its simplicity. However, MSTest doesn't work well in this case,
so I needed to step up to NUnit. The problem is that it won't surface separate scenarios
as separate tests. See [MSTestWiring.fs](https://github.com/fsprojects/TickSpec/blob/master/Examples/ByFramework/MSTest/MSTest.FSharp/MSTestWiring.fs) and [testfx-docs #52](https://github.com/Microsoft/testfx-docs/pull/52).

### Playwright for .NET

The alternative is Selenium using Webdriver. These have a reputation for producing somewhat
unstable tests. Playwright is build on DevTools, which is newer. I've found my Playwright tests
to be perfectly stable, once I got the timeouts correct for the environment I'm on. Overall,
I'm super happy with the ease of use and stability of Playwright.

### TickSpec

Use of TickSpec is probably the most unorthodox choice. SpecFlow is definitely the common choice.
My view is that TickSpec is more lightweight and closer to the metal. SpecFlow tends to abstract
away the details, with IDE extensions, and extra UI. I don't need extra UI. Or more abstractions.

TickSpec also brings the use of F#. For some, a whole new language may be a bit much just to
adopt a test framework, and I understand that. Still, for me, I think F# is pretty cool, and
enjoy learning it a bit more.

### FsUnit

Adopting FsUnit allows for having a consistent coding style to the F#-defined steps. This is
really an optional piece of the stack. Still, I find it helps for overall readability and consistency of the steps.

