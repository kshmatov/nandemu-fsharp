// Virual machine from NAND to Tetris course
open System

// Sample program:
// @2
// D=A
// @3
// D=D+A
// @0
// M=D

let sample = [|0b0000000000000010us;
0b1110110000010000us;
0b0000000000000011us;
0b1110000010010000us;
0b0000000000000000us;
0b1110001100001000us;
0b1111011110111111us;|]

let loadFile path = 
    IO.File.ReadAllText path

let txtTobin (code: string): uint16 array = 
    code.Split "\n" |> Array.filter (fun x -> x.Length > 0)|> Array.map (fun x -> "0b" + x |> uint16)

[<EntryPoint>]
let main argv =
    let rom = 
        if argv.Length > 0 then
            argv[0] |> loadFile |> txtTobin
        else
            sample
    computer.RunComputer true 10 rom
    0