module PortfolioSteps

open NUnit.Framework
open System
open TickSpec
open Microsoft.Playwright
open FsUnit

//
// ASYNC WORKAROUNDS
// TickSpec doesn't support async tests, so each step must be synchronous
//

let Await (x:System.Threading.Tasks.Task) =
    x.Wait()
let AwaitValue x =
    x |> Async.AwaitTask |> Async.RunSynchronously

//
// GIVEN
//

let [<Given>] ``user launched site`` (page: IPage) (uri: Uri) = 
    page.GotoAsync(uri.ToString()) |> AwaitValue

// 
// WHEN
//

let [<When>] ``user launches site`` (page: IPage) (uri: Uri) = 
    ``user launched site`` page uri

let [<When>] ``clicking (.*) in (.*)`` (title:string) (container:string) (page:IPage) =
    page.ClickAsync($"data-test-id={container} >> data-test-id={title}") |> Await

let [<When>] ``waiting for network`` (page:IPage) =
    page.WaitForLoadStateAsync(LoadState.NetworkIdle) |> Await

let [<When>] ``clicking (.*) (.*) times`` (element:string) (count:int) (page:IPage) =
    for _ in 1..count do page.ClickAsync($"data-test-id={element}") |> Await

// 
// THEN
//

let [<Then>] ``(\S*) is (.*)`` (element:string) (expected:string) (page: IPage) =
    page.TextContentAsync($"data-test-id={element}") 
        |> AwaitValue
        |> should equal expected

let [<Then>] ``element (\S*) is (.*)`` (element:string) (expected:string) (page: IPage) =
    page.TextContentAsync(element) 
        |> AwaitValue
        |> should equal expected

let [<Then>] ``page loaded ok`` (response: IResponse) =
    response.Ok 
        |> should be True

let [<Then>] ``page title is (.*)`` (expected:string) (page:IPage) =
    page.TitleAsync() 
        |> AwaitValue
        |> should equal expected

let [<Then>] ``a (\S*) (\S*) is returned`` (testid: string) (element: string) (page: IPage) =
    let locator = page.Locator($"{element}[data-test-id={testid}]")
    locator.WaitForAsync() |> Await
    locator.CountAsync() 
        |> AwaitValue
        |> should equal 1
    locator

let [<Then>] ``it has (.*) columns and (.*) rows`` (columns: int) (rows: int) (table: ILocator) =
    table.Locator("thead >> th").CountAsync() 
        |> AwaitValue
        |> should equal columns
    
    if (rows > 0) then
        table.Locator("tbody >> tr").CountAsync() 
            |> AwaitValue
            |> should equal rows

let [<Then>] ``save a screenshot named (.*)`` (name:string) (page:IPage) =
    let filename = $"Screenshot/{name}.png";
    let options = new PageScreenshotOptions ( Path = filename, FullPage = true, OmitBackground = true )
    page.ScreenshotAsync(options)
        |> AwaitValue
        |> Array.length
        |> should be (greaterThan 100)
    TestContext.AddTestAttachment(filename)

// 
// COMPOSITES
//

let [<Given>] ``user navigated to (.*) page via (.*)`` (title:string) (container:string) (page: IPage) (uri: Uri) = 
    ``user launched site`` page uri |> ``page loaded ok``
    ``clicking (.*) in (.*)`` title container page
    ``waiting for network`` page

let [<When>] ``user navigates to (.*) page via (.*)`` (title:string) (container:string) (page: IPage) (uri: Uri) = 
    ``user navigated to (.*) page via (.*)`` title container page uri
