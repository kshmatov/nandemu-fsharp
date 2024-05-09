// Virual machine from NAND to Tetris course
open System

// Sample program:
// @2
// D=A
// @3
// D=D+A
// @0
// M=D

type cmdParams =
    { debug: bool
      tick: int
      fileName: string option }

let sample =
    [| 0b0000000000000010us
       0b1110110000010000us
       0b0000000000000011us
       0b1110000010010000us
       0b0000000000000000us
       0b1110001100001000us
       0b1111011110111111us |]

let usage () =
    printfn "usage:"
    printfn "$ nandvm [/h] [/d] [/t <value>] [file_name]"
    printfn "/h - show this message and exit"
    printfn "/d - enable DEBUG mod, optional, default false"
    printfn "/t <value> - set cpu tick duration to <value> ms, where value is integer, optional, default 10"

    printfn
        "file_name - path and file name of programm to run, optional. If parameter is empty, sample programm will be executed. Only one file will be executed"

    exit 0

let getTick x =
    try
        x |> int
    with _ ->
        printfn "cant parse tick '%s'" x
        usage ()

let parseCode (a: string) : uint16 =
    try
        "0b" + a.Trim() |> uint16
    with _ ->
        printfn
            "Can't parse line as hackVM instruction. Expected string representation of 16 bit binary value like '1110000011000111'. Got:"

        printfn "%A" a
        exit 0

let rec parseParams (argv: string list) (p: cmdParams) : cmdParams =
    match argv with
    | [] -> p
    | x :: _ when x = "/h" -> usage ()
    | x :: xs when x = "/d" -> parseParams xs { p with debug = true }
    | x :: xs when x = "/t" ->
        match xs with
        | x :: xs -> parseParams xs { p with tick = x |> getTick }
        | _ ->
            printfn "empty tick value, will be 1ms"
            { p with tick = 1 }
    | x :: _ -> { p with fileName = x |> Some }


let loadFile (path: string) : uint16 array =
    path
    |> IO.File.ReadAllText
    |> (fun code -> code.Split "\n")
    |> Array.filter (fun x -> x.Length > 0)
    |> Array.map parseCode

[<EntryPoint>]
let main argv =
    let vmParams =
        parseParams
            (argv |> List.ofArray)
            { debug = false
              tick = 10
              fileName = None }

    let rom =
        match vmParams.fileName with
        | Some fn -> fn |> loadFile
        | _ -> sample

    computer.RunComputer vmParams.debug vmParams.tick rom
    0
