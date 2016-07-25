module Bowling

let (|Digit|_|) char =
    let zero = System.Convert.ToInt32 '0'
    if System.Char.IsDigit char then
        Some (System.Convert.ToInt32 char - zero)
    elif char = '-' then
        Some 0
    else
        None

let rec parseScore (chars: list<char>) : list<Option<int>> =
    match chars with
    | [] -> []
    | 'X' :: rest -> Some 10 :: parseScore rest
    | Digit x :: '/' :: rest -> Some x :: Some (10 - x) :: parseScore rest
    | '-' :: rest -> Some 0 :: parseScore rest
    | Digit x :: rest -> Some x :: parseScore rest
    | _ :: rest -> None :: parseScore rest

let rec countScore (scores: list<int>) : int =
    match scores with
    | [] -> 
        0
    | 10 :: (b1 :: b2 :: tail as rest) ->
        (10 + b1 + b2) + (if tail = [] then 0 else countScore rest)
    | r1 :: r2 :: (b :: tail as rest) when r1 + r2 = 10 ->
        (10 + b) + (if tail = [] then 0 else countScore rest)
    | r1 :: rest ->
        r1 + countScore rest