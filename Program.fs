// Virual machine from NAND to Tetris course
open System
open alu

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

let res = alu.RunSettings sample 1 true

printfn "Sample run: %d" res