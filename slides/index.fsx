(**
- title : Writing .NET applications in F#
- description : Writing .NET applications in F#
- author : Tomasz Heimowski
- theme : night
- transition : default

***

## F# CAMP

### Writing .NET applications in F#

* Open up new instance of **Visual Studio Code**
* Let's do it in .NET Core
* Make sure you have fairly recent .NET Core version by typing "dotnet --version". You should see something like "2.1.403"

---

## Agenda

* F# Library (bowling score)
* F# Console app
* F# Test project - [NUnit](https://nunit.org/)
* C# Console app - integration with F#
* F# Web app - Saturn

***

---

## Ionide recent problem
Race condition when adding new projects to solution and creating references between them.
Workaround - run from `Bowling` folder

```
 Get-ChildItem -path . -recurse -Filter obj | Remove-Item -force -recurse
```

If `access denied` keep trying.
And **Reload Window** until it goes away.

***

## F# Library (bowling score)

* Create new folder for your code and inside create new, **blank** solution called `Bowling`
* Create new F# library called `Bowling` in the solution


    [lang=cmd]
    > mkdir Bowling
    > cd Bowling
    > dotnet new sln --name Bowling
    > dotnet new classlib --language F# --name Bowling
    > dotnet sln add Bowling
    > dotnet build

---

* Open `Bowling` folder in Visual Studio Code
* Rename `Library.fs` to `Bowling.fs`. Don't forget to rename entry in `Bowling.fsproj`!
* Open renamed file `Bowling.fs` in editor
* Remove generated code from the file, and insert namespace declaration:


    namespace Bowling
    module Api =

---

* Copy code for `Digit` active pattern recognizer into `Bowling.Api` module indented by 4 whitespaces *)
let (|Digit|_|) char =
    let zero = System.Convert.ToInt32 '0'
    if System.Char.IsDigit char then
        Some (System.Convert.ToInt32 char - zero)
    else
        None

(**

---
* Copy code for `parseScore` function after `Digit`*)
let rec parseScore (chars: char list) : int option list =
    match chars with
    | [] -> []
    | 'X' :: rest -> Some 10 :: parseScore rest
    | Digit x :: '/' :: rest -> Some x :: Some (10 - x) :: parseScore rest
    | Digit x :: rest -> Some x :: parseScore rest
    | '-' :: '/' :: rest -> Some 0 :: Some 10 :: parseScore rest
    | '-' :: rest -> Some 0 :: parseScore rest
    | _ :: rest -> None :: parseScore rest

(**

#### ! Remember to save all changes when manipulating projects in Visual Studio Code (Ctrl + K + S)

---

### Test the code in Interactive

* Select all lines **excluding** namepspace and module declarations
* Trigger *Execute in Interactive* by pressing *Alt + Enter*
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

let countScore (scores: int list) : int =
    let rec count frame scores =
        match scores with
        | [] -> 0
        | 10 :: (b1 :: b2 :: _ as next) ->
            10 + b1 + b2 + (if frame = 10 then 0 else count (frame+1) next)
        | r1 :: r2 :: (b1 :: _ as next) when r1 + r2 = 10 ->
            10 + b1 +      (if frame = 10 then 0 else count (frame+1) next)
        | r1 :: r2 :: next ->
            r1 + r2 + count (frame+1) next
        | _ -> failwith "invalid score"
    count 1 scores


(**
* Test the function in interactive:
*)
let countScoreResult = countScore [10;9;1;5;5;7;2;10;10;10;9;0;8;2;9;1;10];;
(** #### Value of ``countScoreResult`` *)
(*** include-value: ``countScoreResult`` ***)
(**

---
* Add `sequenceOpts` function *)
let sequenceOpts (optionals: 'a option list) : 'a list option =
    let rec sequenceOpts' acc optionals =
        match optionals, acc with
        | [],_ ->
            Option.map List.rev acc
        | Some h :: t, Some acc ->
            sequenceOpts' (Some (h :: acc)) t
        | _ ->
            None

    sequenceOpts' (Some []) optionals

(**
* Test the function in interactive:
*)
let oneOption = sequenceOpts [Some "abc"; Some "def"; Some "ghi"];;
(** #### Value of ``oneOption`` *)
(*** include-value: ``oneOption`` ***)
(**


---

* Add `bowlingScore` function *)

let bowlingScore (score: string) : int option =
    score.ToCharArray()
    |> Array.toList
    |> parseScore
    |> sequenceOpts
    |> Option.map countScore

(**
* Test the function in interactive:
*)
let bowlingScoreResult = bowlingScore "X9/5/72XXX9-8/9/X";;
(** #### Value of ``bowlingScoreResult`` *)
(*** include-value: ``bowlingScoreResult`` ***)
(**

---

### Save & build

    [lang=cmd]
    > dotnet build

---

### Summary

* Creating F# Library projects in VSC
* Declaring Bowling namespace and module
* Testing code in interactive

---

### Links

* [Installing and using F#](https://fsharpforfunandprofit.com/installing-and-using/) by Scott Wlaschin
* [F# coding conventions](https://docs.microsoft.com/pl-pl/dotnet/fsharp/style-guide/conventions)
* [Organizing functions - Nested functions and modules](https://fsharpforfunandprofit.com/posts/organizing-functions/) by Scott Wlaschin

***

## F# Console app

* Create new F# Console Application `Bowling.Console` and add to solution
* Add **project reference** from `Bowling.Console` to `Bowling`
* Build "bowling" solution


    [lang=cmd]
    > dotnet new console --language F# --name Bowling.Console
    > dotnet sln add Bowling.Console
    > dotnet add Bowling.Console reference Bowling
    > dotnet build


---

* Invoke `Bowling.Api.bowlingScore` on example input and print output


    // Learn more about F# at http://fsharp.org
    // See the 'F# Tutorial' project for more help.

    [<EntryPoint>]
    let main argv =
        printfn "%A" (Bowling.Api.bowlingScore "XXXXXXXXXXXX")
        0 // return an integer exit code


---

* Run it!

![some_300.png](images/some_300.png)

---

### Exercise

Invoke `Bowling.Api.bowlingScore` for each argument from `argv` (console arguments)

    XXXXXXXXXXXXX 9-9-9-9-9-9-9-9-9-9- 5/5/5/5/5/5/5/5/5/5/5 X9/5/72XXX9-8/9/X

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


---

## F# Test project - [nUnit](https://nunit.org/)

* Create new F# Library `Bowling.Tests`
* Tests are like console applications - must target a platform


    [lang=cmd]
    > dotnet new classlib --language F# --name Bowling.Tests --framework netcoreapp2.1
    > dotnet sln add Bowling.Tests
    > dotnet add Bowling.Tests reference Bowling
    > dotnet build


* Rename `Library.fs` to `Tests.fs`. Don't forget *.fsproj!
* Open `Tests.fs` and remove boilerplate code and declare top level `Bowling.Tests` module:


    [lang=fs]
    module Bowling.Tests

#### ! Save all changes in Visual Studio Code

---

### [Paket](http://fsprojects.github.io/Paket/) for managing dependencies

* Create new directory *.paket* next to the *.sln* solution file
* Download *paket.bootstrapper.exe* from [here](https://github.com/fsprojects/Paket/releases/download/5.181.1/paket.bootstrapper.exe) and save it in ".paket" directory
* Rename *.paket\paket.bootstrapper.exe* to *.paket\paket.exe*
* In console, change directory to *.\Bowling* main directory where *Bowling.sln* is
* Run `paket.exe init`:


    [lang=cmd]
    > mkdir .paket
    > cd .paket
    > wget https://github.com/fsprojects/Paket/releases/download/5.181.1/paket.bootstrapper.exe
    > mv paket.bootstrapper.exe paket.exe
    > cd ..
    > .paket\paket.exe init

---

* Open *paket.dependencies* in VSC editor,
* add option `storage: none` to mirror NuGet behavior and disable the packages folder and use the global NuGet cache


    [lang=paket]
    source https://www.nuget.org/api/v2
    storage: none

---

* Add NUnit, Microsoft.NET.Test.Sdk, Unquote references and place dependency in separate paket group `Tests`


    [lang=cmd]
    > .paket\paket.exe add NUnit --project Bowling.Tests --group Tests
    > .paket\paket.exe add NUnit3TestAdapter --project Bowling.Tests --group Tests
    > .paket\paket.exe add Microsoft.NET.Test.Sdk --project Bowling.Tests --group Tests
    > .paket\paket.exe add FsUnit --project Bowling.Tests --group Tests
    > .paket\paket.exe add Unquote --project Bowling.Tests --group Tests


* Look inside global `paket.dependencies` file and project's local `paket.references` file


#### Alternative
Edit `paket.dependencies` and `paket.references` manually and run:


    [lang=paket]
    .paket/paket.exe install

---

* Open `Tests.fs` source file in editor,
* Add unit test for checking score of 12 Strikes:


    [lang=fs]
    module Bowling.Tests

    open NUnit.Framework
    open FsUnit

    [<Test>]
    let ``12 strikes in row``() =
        let expected = Some 270
        let actual = Bowling.Api.bowlingScore "XXXXXXXXXXXX"
        actual |> should equal expected


---

* Run the tests:


    [lang=cmd]
    > dotnet test Bowling.Tests


![test_failure.png](images/test_failure.png)


* Fix the test and run it again

---

* Rewrite the failing test using `Unquote` library and `quoted expressions`


    [lang=fs]
    open Swensen.Unquote
    [<Test>]
    let ``12 strikes in row``() =
        test<@ Bowling.Api.bowlingScore "XXXXXXXXXXXX" = Some 270 @>


* Fix the test and run it again

---

### Exercises

* Add three more test cases for following scores using either Unquote or standard assertions:
    * "9-9-9-9-9-9-9-9-9-9-"
    * "5/5/5/5/5/5/5/5/5/5/5"
    * "X9/5/72XXX9-8/9/X"

---

### Demo. REPL driven testing

* Build solution so that there is *Bowling.dll*
* Use paket to generate load scripts - very helpful!


    [lang=cmd]
    > .paket\paket.exe  generate-load-scripts --framework netcoreapp2.1

* Create `Script.fsx` in bowling.test


    [lang=fs]
    #r @"bin/Debug/netcoreapp2.1/Bowling.dll"
    #load @"../.paket/load/netcoreapp2.1/Tests/tests.group.fsx"

    match Bowling.Api.bowlingScore "XXXXXXXXXXXX" with
    | Some score -> printfn "Score is %d" score
    | None -> printfn "Wrong score"


---

* Run in VSC (highlight + Alt + Enter ) and in command line


    [lang=cmd]
    > fsi Bowling.Tests\Script.fsx


* Reference file directly for even tighter feedback


    [lang=fs]
    #load @"../Bowling/Bowling.fs"

---

### Summary

* Creating test library in F#
* Adding test nuget packages with Paket
* Writing unit tests in F#
* REPL driven testing

---

### Links

* [Paket](http://fsprojects.github.io/Paket/)
* [Paket and .NET Core](https://fsprojects.github.io/Paket/paket-and-dotnet-cli.html)
* [FsUnit project](https://fsprojects.github.io/FsUnit/)
* [Using F# for testing](https://fsharpforfunandprofit.com/posts/low-risk-ways-to-use-fsharp-at-work-3/) by Scott Wlaschin
* [An introduction to property-based testing](https://fsharpforfunandprofit.com/posts/property-based-testing/) by Scott Wlaschin
* [Unquote project](http://www.swensensoftware.com/unquote)

***

## C# code integration with F#

---

* Create new C# Console Application `Bowling.Csharp` and add to solution
* Add **project reference** from `Bowling.Csharp` to `Bowling`
* Build "bowling" solution


    [lang=cmd]
    > dotnet new console --language C# --name Bowling.Csharp
    > dotnet sln add Bowling.Csharp
    > dotnet add Bowling.Csharp reference Bowling
    > dotnet build

---

* To integrate with F# Option type reference FSharp.Core from C#


    [lang=cmd]
    > .paket\paket.exe add FSharp.Core --project Bowling.Csharp


---

* Replace Program.cs content in `Bowling.Csharp` with the following:


    [lang=csharp]
    using System;
    using Microsoft.FSharp.Core;

    namespace Bowling.CSharp
    {
        class Program
        {
            static void Main(string[] args)
            {
                foreach(var input in args) {
                    var score = Bowling.Api.bowlingScore(input);
                    var scoreStr =
                        FSharpOption<int>.get_IsSome(score) ?
                        "Score: " + score.Value :
                        "Wrong score!";
                    Console.WriteLine(scoreStr);
                }
            }
        }
    }

---

Build and Run it!

![csharp_runit.png](images/csharp_runit.png)

---

### The F# Component Design Guidelines

#### Below snippet doesn't feel nice in C#:

    [lang=csharp]
    var scoreStr =
        FSharpOption<int>.get_IsSome(score) ?
        "Score: " + score.Value :
        "Wrong score!";

---

### The F# Component Design Guidelines

https://docs.microsoft.com/pl-pl/dotnet/fsharp/style-guide/component-design-guidelines

> Use the TryGetValue pattern instead of returning F# option values, and prefer method overloading to taking F# option values as arguments.

This tick can be found [in this section](https://docs.microsoft.com/pl-pl/dotnet/fsharp/style-guide/component-design-guidelines#use-the-trygetvalue-pattern-instead-of-returning-f-option-values-and-prefer-method-overloading-to-taking-f-option-values-as-arguments) of above guidelines.

---


### Exercise

Create new function `TryGetBowlingScore` in `Bowling` module for better interop with C#, conforming to the F# Component Design Guidelines.
Use the new function in code behind button click in C#.

#### Skeleton of the function


    [lang=fsharp]
    open System.Runtime.InteropServices

    let TryGetBowlingScore(score: string, [<Out>] result : byref<int>) : bool =
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

* [The F# Component Design Guidelines](https://docs.microsoft.com/pl-pl/dotnet/fsharp/style-guide/component-design-guidelines/)
* [Seamless interoperation with .NET libraries](https://fsharpforfunandprofit.com/posts/completeness-seamless-dotnet-interop/) by Scott Wlaschin


***

## F# Web app

---

* Create new F# Console Application `Bowling.Web` and add to solution
* Add **project reference** from `Bowling.Web` to `Bowling`
* Add `Saturn` framework dependency
* Build "bowling" solution


    [lang=cmd]
    > dotnet new console --language F# --name Bowling.Web
    > dotnet sln add Bowling.Web
    > dotnet add Bowling.Web reference Bowling
    > .paket\paket.exe add Saturn --project Bowling.Web
    > dotnet build

---

* Open `Program.fs` from `Bowling.Web`,
* Remove boilerplater code, and insert following hello world Saturn:


    [lang=fsharp]
    module Bowling.Web

    open Saturn
    open Giraffe

    let helloWorldName str = text ("hello world, " + str)

    let topRouter = router {
        not_found_handler (setStatusCode 404 >=> text "404")
        getf "/name/%s" helloWorldName
    }

    let app = application {
        use_router topRouter
        url "http://localhost:8085/"
    }

    [<EntryPoint>]
    let main _ =
        run app
        0

---

Run it!

![hello_world_saturn.png](images/hello_world_saturn.png)

---

###Exercise

Implement `scoreHandler` function similar to `helloWorldName` so that:

* it responds with 200 OK with score for correct input,
* it responds with 400 BAD REQUEST with "Wrong result" message for wrong input


Hint: Make use of `Successful.OK` and `RequestErrors.BAD_REQUEST` functions. Both take generic input.

---

## Remember: .fs files order in project matter!

At first it looks like a limitation but it really turns out to be one of the most **beloved** F# features

---

### Summary

* Saturn is a web framework, easy to use with F#. Sits on top of ASP.NET Core, Kestrel and Giraffe
* .fs file order inside project matters

---

### Links

* [Saturn](https://saturnframework.org/)
* [Cyclic dependencies are evil](https://fsharpforfunandprofit.com/posts/cyclic-dependencies) by Scott Wlaschin

***

## Summary

* F# Library (bowling score)
* F# Console app
* F# Test project - [NUnit](https://nunit.org/)
* C# Console app - integration with F#
* F# Web app - Saturn

*)