open Suave

let scoreHandler input =
    match Bowling.bowlingScore input with
    | Some score -> Successful.OK (score.ToString())
    | None -> RequestErrors.BAD_REQUEST "wrong input"

startWebServer defaultConfig (Filters.pathScan "/%s" scoreHandler)