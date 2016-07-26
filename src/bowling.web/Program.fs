open Suave
open Suave.Filters

let scoreHandler input =
    match Bowling.bowlingScore input with
    | Some score -> Successful.OK (score.ToString())
    | None -> RequestErrors.BAD_REQUEST "wrong input"

startWebServer defaultConfig (pathScan "/%s" scoreHandler)