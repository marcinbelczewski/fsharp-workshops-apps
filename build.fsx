#r @"packages/Build/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Testing

Target "Build" (fun _ ->
    MSBuildRelease "bin" "Build" ["src/bowling.sln"]
    |> Log "Build output"
)

Target "Tests" (fun _ ->
    ["bin/bowling.tests.dll"]
    |> xUnit2 (fun xunitParams -> 
        { xunitParams with ToolPath = @"packages/Build/xunit.runner.console/tools/xunit.console.exe" }
    )
)

"Build"
    ==> "Tests"

RunTargetOrDefault "Tests"