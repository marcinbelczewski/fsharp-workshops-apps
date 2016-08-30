(**
- title : Writing .NET applications in F#
- description : Writing .NET applications in F#
- author : Tomasz Heimowski
- theme : night
- transition : default

***

## F# CAMP

### Writing .NET applications in F#

* Open up new instance of **Visual Studio 2015**
* Make sure you have F# templates (in `Other Languages`)
* Let's stick to .NET Framework v4.5.1

![fsharp_templates.png](images/fsharp_templates.png)

---

## Agenda

* F# Library (bowling score)
* F# Console app
* F# Build script - [FAKE](http://fsharp.github.io/FAKE/)
* F# Test project - [xUnit](https://xunit.github.io/)
* C# Window app - WPF (integration with F#)
* F# Web app

***

## F# Library (bowling score)

* Create new, **blank** solution called "bowling" in directory of your choice 

![blank_sln.png](images/blank_sln.png)

* Create new F# library called "bowling" in the solution

---

* Delete `Script.fsx` from the project
* Rename `Library1.fs` to `Bowling.fs`
* Open renamed file `Bowling.fs` in editor
* Remove generated code from the file, and insert module declaration:


    module Bowling


#### ! Remember to save all changes when manipulating projects in Visual Studio (Ctrl + Shift + S)

---

* Copy code for `Digit` active pattern recognizer into `Bowling` module *)

let (|Digit|_|) char =
    let zero = System.Convert.ToInt32 '0'
    if System.Char.IsDigit char then
        Some (System.Convert.ToInt32 char - zero)
    else
        None

(**

---

 * Copy code for `parseScore` function after `Digit`*)

let rec parseScore (chars: list<char>) : list<Option<int>> =
    match chars with
    | [] -> []
    | 'X' :: rest -> Some 10 :: parseScore rest
    | Digit x :: '/' :: rest -> Some x :: Some (10 - x) :: parseScore rest
    | Digit x :: rest -> Some x :: parseScore rest
    | '-' :: '/' :: rest -> Some 0 :: Some 10 :: parseScore rest
    | '-' :: rest -> Some 0 :: parseScore rest
    | _ :: rest -> None :: parseScore rest

(**

---

### Test the code in Interactive

* Select all lines **excluding** module declaration
* Trigger *Execute in Interactive*
* In interactive window, enter following:
*)

let parseScoreResult = parseScore ['4'; '/'];;
(** #### Value of ``parseScoreResult`` *)
(*** include-value: ``parseScoreResult`` ***) 
(**
    
---

### Exercise

* Extend `Digit` active pattern to recognize '-' character as well,
* Rename `Digit` to `Pins` to better reflect its intent after the change,
* Refactor `parseScore` function - make use of the fact that `Pins` recognizes now '-' and remove redundant pattern matching case(s),
* In interactive, make sure that after refactoring the code still works.  

---

* Add `countScore` function *)

let rec countScore (scores: list<int>) : int =
    match scores with 
    | [] -> 0
    | 10 :: (b1 :: b2 :: tail as next) ->
        10 + b1 + b2 + (if tail = [] then 0 else countScore next)
    | r1 :: r2 :: (b1 :: tail as next) when r1 + r2 = 10 ->
        10 + b1 +      (if tail = [] then 0 else countScore next)
    | r1 :: r2 :: next ->
        r1 + r2 + countScore next

(**
* Test the function in interactive:
*)
let countScoreResult = countScore [10;9;1;5;5;7;2;10;10;10;9;0;8;2;9;1;10];;
(** #### Value of ``countScoreResult`` *)
(*** include-value: ``countScoreResult`` ***) 
(**

---

* Add `optsToOpt` function *)

let optsToOpt (opts : List<Option<'a>>) : Option<List<'a>>  =
    let rec optsToOpt' acc opts =
        match acc, opts with
        | x, [] -> x |> Option.map List.rev
        | Some xs, Some x :: rest ->
            optsToOpt' (Some (x :: xs)) rest
        | _ -> None

    optsToOpt' (Some []) opts

(**
* Test the function in interactive:
*)
let oneOption = optsToOpt [Some "abc"; Some "def"; Some "ghi"];;
(** #### Value of ``oneOption`` *)
(*** include-value: ``oneOption`` ***) 
(**


---

* Add `bowlingScore` function *)

let bowlingScore (score: string) : Option<int> =
    score.ToCharArray()
    |> Array.toList
    |> parseScore
    |> optsToOpt
    |> Option.map countScore

(**
* Test the function in interactive:
*)
let bowlingScoreResult = bowlingScore "X9/5/72XXX9-8/9/X";;
(** #### Value of ``bowlingScoreResult`` *)
(*** include-value: ``bowlingScoreResult`` ***) 
(**

---

### Summary

* Creating F# Library projects in VS
* Declaring Bowling module
* Testing code in interactive

---

### Links 

* [Installing and using F#](https://fsharpforfunandprofit.com/installing-and-using/) by Scott Wlaschin
* [Organizing functions - Nested functions and modules](https://fsharpforfunandprofit.com/posts/organizing-functions/) by Scott Wlaschin

***

## F# Console app

* Create new F# Console Application "bowling.console"
* Add **project reference** from "bowling.console" to "bowling"
* Compile "bowling" project

---

* Invoke `Bowling.bowlingScore` on example input and print output


    // Learn more about F# at http://fsharp.org
    // See the 'F# Tutorial' project for more help.

    [<EntryPoint>]
    let main argv = 
        printfn "%A" (Bowling.bowlingScore "XXXXXXXXXXXX")
        0 // return an integer exit code


---

* Run it!

![some_300.png](images/some_300.png)

---

### Exercise 

Invoke `Bowling.bowlingScore` for each argument from `argv` (console arguments)

![array_iter.png](images/array_iter.png)

Hint: Use `Array.iter` function to perform an action for each element from an array


---

### Summary

* Creating F# console apps
* Printing to console

---

### Links

* [Formatted text using printf](https://fsharpforfunandprofit.com/posts/printf/) by Scott Wlaschin

***

## F# Build script - [FAKE](http://fsharp.github.io/FAKE/)

### [Paket](http://fsprojects.github.io/Paket/) for managing dependencies

---

* Create new directory ".paket" next to the ".sln" solution file
* Download paket.bootstrapper.exe from [here](https://github.com/fsprojects/Paket/releases/download/3.9.5/paket.bootstrapper.exe) and save it in ".paket" directory
* In console, change directory to where the solution file and ".paket" folder are. **Do not** change directory to ".paket"
* Run paket.bootstrapper.exe from console to download newest Paket, and then invoke `paket.exe init`:


    [lang=cmd]
    > .paket\paket.bootrapper.exe
    > .paket\paket.exe init


---

* In solution, add "New Solution Folder" called ".project"
* "Add Existing Item" - add "paket.dependencies" file to the ".project" solution folder
* Open "paket.dependencies" file in the VS editor

---

Modify the "paket.dependencies" file to add "Build" group and "FAKE" package:


    [lang=paket]
    group Build
        source https://www.nuget.org/api/v2

        nuget FAKE

---

Run paket install:


    [lang=cmd]
    > .paket\paket.exe install

---

Add "New Item", "build.cmd" to the ".project" solution folder:


    [lang=cmd]
    @echo off
    cls
    .paket\paket.bootstrapper.exe
    .paket\paket.exe restore
    packages\Build\FAKE\tools\FAKE.exe build.fsx %*

--- 

Add "New Item", "build.fsx" to the ".project" solution folder:


    #r @"packages/Build/FAKE/tools/FakeLib.dll"

    open Fake

    Target "Build" (fun _ ->
        MSBuildRelease "bin" "Build" ["bowling.sln"]
        |> Log "Build output"
    )

    RunTargetOrDefault "Build"

---

Run the build script:

    
    build.cmd

![build_hello_fake.png](images/build_hello_fake.png)

---

### Summary

* Paket for managing dependencies
* FAKE for build scripts
* Invoking build script from command line

---

### Links

* [FAKE](http://fsharp.github.io/FAKE/)
* [Paket](http://fsprojects.github.io/Paket/)
* [ProjectScaffold](https://fsprojects.github.io/ProjectScaffold/)

***

## F# Test project - [xUnit](https://xunit.github.io/)

---

* Create new F# Library "bowling.tests" for .NET **4.5.1**,
* Remove "Script.fsx" file,
* Rename "Library1.fs" to "Tests.fs",
* "Add new item", "App.config" application configuration file to "bowling.tests",
* Add Project Reference from "bowling.tests" to "bowling",
* Remove boilerplate code and declare `Bowling.Tests` module:


    module Bowling.Tests

#### ! Save all changes in Visual Studio

---

* Open "paket.dependencies" in VS editor,
* Add "xunit.runner.console" package to "Build" group,
* Add new group "Tests" with "framework: net451",
* Add "FSharp.Core" with "redirects: force" option, "xUnit" and "FsUnit.xUnit" nugets to "Tests" group


    [lang=paket]
    group Build
        source https://www.nuget.org/api/v2

        nuget FAKE
        nuget xunit.runner.console

    group Tests
        framework: net451
        source https://www.nuget.org/api/v2
        
        nuget FSharp.Core redirects: force
        nuget xUnit
        nuget FsUnit.xUnit

---

* Add "New Item", "paket.references" (General -> Text File) to "bowling.tests" project
* Open "paket.references" file in VS editor and fill it with below:


    [lang=paket]
    group Tests
        FSharp.Core
        xUnit
        FsUnit.xUnit

---

Run "paket install":


    [lang=cmd]
    > .paket\paket.exe install

---

* Open "build.fsx" build script in VS editor,
* Add "Tests" build target:


    [lang=fsharp]
    open Fake.Testing // for testing helper functions

    Target "Tests" (fun _ ->
        ["bin/bowling.tests.dll"]
        |> xUnit2 (fun xunitParams -> 
            { xunitParams with ToolPath = @"packages/Build/xunit.runner.console/" 
                                        + @"tools/xunit.console.exe" }
        )
    )


---

At the bottom of "build.fsx", specify Target dependency and change default target to "Tests":


    [lang=fsharp]
    // Targets above    
    
    "Build"
        ==> "Tests"

    RunTargetOrDefault "Tests"

---

    [lang=fsharp]
    #r @"packages/Build/FAKE/tools/FakeLib.dll"

    open Fake
    open Fake.Testing

    Target "Build" (fun _ ->
        MSBuildRelease "bin" "Build" ["bowling.sln"]
        |> Log "Build output"
    )

    Target "Tests" (fun _ ->
        ["bin/bowling.tests.dll"]
        |> xUnit2 (fun xunitParams -> 
            { xunitParams with ToolPath = @"packages/Build/xunit.runner.console/" 
                                        + @"tools/xunit.console.exe" }
        )
    )

    "Build"
        ==> "Tests"

    RunTargetOrDefault "Tests"

---

* Open "Tests.fs" source file in editor,
* Add unit test for checking score of 12 Strikes:


    [lang=fs]
    module Bowling.Tests

    open Xunit
    open FsUnit.Xunit
        
    [<Fact>]
    let ``12 strikes in row`` () =
        let expected = Some 270
        let actual = Bowling.bowlingScore "XXXXXXXXXXXX"
        actual |> should equal expected

---

Run the build script (without any additional parameters):

![test_failure.png](images/test_failure.png)

---

### Exercises

* Fix the test
* Add three more test cases for following scores:
    * "9-9-9-9-9-9-9-9-9-9-"
    * "5/5/5/5/5/5/5/5/5/5/5"
    * "X9/5/72XXX9-8/9/X"

--- 

### Summary

* Creating test library in F#
* Adding test nuget packages with Paket
* Attaching tests to the build script pipeline
* Writing unit tests in F#

---

### Links

* [FsUnit project](https://fsprojects.github.io/FsUnit/)
* [Using F# for testing](https://fsharpforfunandprofit.com/posts/low-risk-ways-to-use-fsharp-at-work-3/) by Scott Wlaschin
* [An introduction to property-based testing](https://fsharpforfunandprofit.com/posts/property-based-testing/) by Scott Wlaschin
* [Unquote project](http://www.swensensoftware.com/unquote)

***

## C# Window app - WPF (integration with F#)

---

* Add C# Windows "WPF Application" project, "bowling.wpf" to the solution,
* Add project reference from "bolwing.wpf" to "bowling",
* Design awesome GUI with a TextBox, TextBlock and a Button:

![gui.png](images/gui.png)

---

* Open "paket.dependencies" file,
* Add "FSharp.Core" package with "redirects: force" option to main group:


    [lang=paket]
    source https://www.nuget.org/api/v2

    nuget FSharp.Core redirects: force

    group Build
        source https://www.nuget.org/api/v2

        nuget FAKE
        nuget xunit.runner.console

    group Tests
        framework: net451
        source https://www.nuget.org/api/v2
        
        nuget FSharp.Core redirects: force
        nuget xUnit
        nuget FsUnit.xUnit

---

Add "New Item", "paket.references" to "bowling.wpf":

    [lang=paket]
    FSharp.Core

---

Run paket install:


    [lang=cmd]
    > .paket\paket.exe install

---

Add action on button click:


    [lang=csharp]
    private void button_Click(object sender, RoutedEventArgs e)
    {
        var input = textBox.Text;
        var score = Bowling.bowlingScore(input);
        textBlock.Text = 
            FSharpOption<int>.get_IsSome(score) ? 
            "Score: " + score.Value.ToString() : 
            "Wrong score!";
    }

---

Run it!

![wpf_runit.png](images/wpf_runit.png)

---

### The F# Component Design Guidelines

#### Below snippet doesn't feel nice in C#:

    [lang=csharp]
    textBlock.Text = 
        FSharpOption<int>.get_IsSome(score) ? 
        "Score: " + score.Value.ToString() : 
        "Wrong score!";

---

### The F# Component Design Guidelines

http://fsharp.org/specs/component-design-guidelines/

> ✔ Consider using the TryGetValue pattern instead of returning F# option values (option) in vanilla .NET APIs, and prefer method overloading to taking F# option values as arguments.

This tick can be found [in this section](http://fsharp.org/specs/component-design-guidelines#object-and-member-design--for-libraries-for-use-from-other-net-languages) of above guidelines.

---


### Exercise 

Create new function `TryGetBowlingScore` in `Bowling` module for better interop with C#, conforming to the F# Component Design Guidelines.
Use the new function in code behind button click in C#.

#### Skeleton of the function


    [lang=fsharp]
    let TryGetBowlingScore(score: string, result : byref<int>) : bool = 
        result <- 0
        false

#### New stuff - assign value operator

    
    [lang=fsharp]
    let mutable x = 0
    x <- 5

---

### Using .NET libraries from F#


    [lang=fsharp]
    let (i1success,i1) = System.Int32.TryParse("123");
    if i1success then printfn "parsed as %i" i1 else printfn "parse failed"

    let dict = new System.Collections.Generic.Dictionary<string,string>()
    dict.Add("a","hello")
    let (e1success,e1) = dict.TryGetValue("a")
    let (e2success,e2) = dict.TryGetValue("b")

    let makeResource name = 
        { new System.IDisposable 
            with member this.Dispose() = printfn "%s disposed" name }

https://fsharpforfunandprofit.com/posts/completeness-seamless-dotnet-interop/

---

### Summary 

* Referencing F# code from C# (FSharp.Core package)
* Conforming to the F# Component Design Guidelines
* Using .NET libraries from F#

---

### Links

* [The F# Component Design Guidelines](http://fsharp.org/specs/component-design-guidelines/)
* [Seamless interoperation with .NET libraries](https://fsharpforfunandprofit.com/posts/completeness-seamless-dotnet-interop/) by Scott Wlaschin


***

## F# Web app

---

* Add F# Console application project, "bowling.web" to the solution,
* Add project reference from "bolwing.web" to "bowling",

---

* Open "paket.dependencies" file,
* Add "Suave" package to main group:


    [lang=paket]
    source https://www.nuget.org/api/v2

    nuget FSharp.Core redirects: force
    nuget Suave

    group Build
        source https://www.nuget.org/api/v2

        nuget FAKE
        nuget xunit.runner.console

    group Tests
        framework: net451
        source https://www.nuget.org/api/v2

        nuget FSharp.Core redirects: force
        nuget xUnit
        nuget FsUnit.xUnit
---

Add "New Item", "paket.references" to "bowling.web":

    [lang=paket]
    Suave

---

Run paket install:


    [lang=cmd]
    > .paket\paket.exe install

---

* Open "Program.fs" from "bowling.web",
* Remove boilerplater code, and insert following hello world suave:


    [lang=fsharp]
    open Suave

    startWebServer defaultConfig (Successful.OK "Hello world")

---

Run it!

![hello_world_suave.png](images/hello_world_suave.png)

---

###Exercise

Implement `scoreHandler` function so that:

* it responds with 200 OK with score for correct input,
* it responds with 400 BAD REQUEST with "Wrong result" message for wrong input:



    [lang=fsharp]
    open Suave

    let scoreHandler (input: string) : WebPart =
        Successful.OK "Hello world"

    startWebServer defaultConfig (Filters.pathScan "/%s" scoreHandler)

Hint: Make use of `Successful.OK` and `RequestErrors.BAD_REQUEST` functions. Both are of type `string -> WebPart`.

---

## Demo: .fs files order in project
### (order matters)

At first it looks like a limitation but it really turns out to be one of the most **beloved** F# features

---

### Summary 

* Suave.IO is a very light-weight server library, easy to use with F#
* .fs file order inside project matters

---

### Links

* [Suave](https://suave.io/)
* [Suave Music Store Gitbook](https://theimowski.gitbooks.io/suave-music-store) by Tomasz Heimowski
* [Suave as a service with Topshelf](http://blog.2mas.xyz/suave-as-a-service-with-topshelf/) by Tomas Jansson
* [Cyclic dependencies are evil](https://fsharpforfunandprofit.com/posts/cyclic-dependencies) by Scott Wlaschin

***

## Summary

* F# Library (bowling score)
* F# Console app
* F# Build script - [FAKE](http://fsharp.github.io/FAKE/)
* F# Test project - [xUnit](https://xunit.github.io/)
* C# Window app - WPF (integration with F#)
* F# Web app

*)