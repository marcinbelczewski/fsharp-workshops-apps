// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

[<EntryPoint>]
let main argv = 
    argv
    |> Array.map Bowling.bowlingScore
    |> Array.iter (printfn "%A")
    0 // return an integer exit code
