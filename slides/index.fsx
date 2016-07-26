(**
- title : Writing .NET applications in F#
- description : Writing .NET applications in F#
- author : Tomasz Heimowski
- theme : night
- transition : default

***

## F# CAMP

### Writing .NET applications in F#

* Open up new instance of Visual Studio 2015
* Make sure you have F# templates (in `Other Languages`)

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

---

* Copy code for `Digit` active pattern recognizer in `Bowling` module *)

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
    | '-' :: '/' :: rest -> Some 0 :: Some 10 :: parseScore rest
    | '-' :: rest -> Some 0 :: parseScore rest
    | Digit x :: rest -> Some x :: parseScore rest
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
* Rename `Digit` to `Roll` to better reflect its intent,
* Refactor `parseScore` function,
* In interactive, make sure that after refactoring the code still works.  

---

* Add `countScore` function *)

let rec countScore (scores: list<int>) : int =
    match scores with
    | [] -> 
        0
    | 10 :: (b1 :: b2 :: tail as rest) ->
        (10 + b1 + b2) + (if List.isEmpty tail then 0 else countScore rest)
    | r1 :: r2 :: (b :: tail as rest) when r1 + r2 = 10 ->
        (10 + b) + (if List.isEmpty tail then 0 else countScore rest)
    | r1 :: rest ->
        r1 + countScore rest

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

### TODO: Use C# code in F#

---

### Summary

* Creating F# console apps
* Printing to console
* Using C# code from F#

***

## F# Build script - [FAKE](http://fsharp.github.io/FAKE/)

### [Paket](http://fsprojects.github.io/Paket/) for managing dependencies

---

* Create new directory ".paket" next to the ".sln" solution file
* Download paket.bootstrapper.exe from [here](https://github.com/fsprojects/Paket/releases/download/3.9.5/paket.bootstrapper.exe) and save it in ".paket" directory
* Run paket.bootstrapper.exe from console to download newest Paket, and then invoke `paket.exe init`:


    [lang=cmd]
    .paket\paket.bootrapper.exe
    .paket\paket.exe init


---

* In solution, add "New Solution Folder" called ".paket"
* "Add Existing Item" - add "paket.dependencies" file to the ".paket" solution folder
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
    .paket\paket.exe install

---

Create "build.cmd" script next to "paket.dependencies":


    [lang=cmd]
    @echo off
    cls
    .paket\paket.bootstrapper.exe
    .paket\paket.exe restore
    packages\Build\FAKE\tools\FAKE.exe build.fsx %*

--- 

Create "build.fsx" script next to "build.cmd":


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

***

## F# Test project - [xUnit](https://xunit.github.io/)

***

## C# Window app - WPF (integration with F#)

http://fsharp.org/specs/component-design-guidelines/

***

## F# Web app

// TODO: Show ordering fs files in proj

***

## Summary

* F# Library (bowling score)
* F# Console app
* F# Build script - [FAKE](http://fsharp.github.io/FAKE/)
* F# Test project - [xUnit](https://xunit.github.io/)
* C# Window app - WPF (integration with F#)
* F# Web app

*)