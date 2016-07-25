#r @"packages/Build/FAKE/tools/FakeLib.dll"

open Fake

Target "Build" (fun _ ->
    MSBuildRelease "bin" "Build" ["src/bowling.sln"]
    |> Log "Build output"
)

RunTargetOrDefault "Build"