module memory

open System

type Instruction = uint16
type Value = int16
type Address = uint16

let MemorySize = 2. **16 |> int

type State = 
    {
    Data: Value array
    Instructions: Instruction array}
    member this.SetMem (a: Address) (v: Value):unit = 
        this.Data[int a] <- v

    member this.GetMem (a: Address): Value = this.Data[int a]

    member this.GetInstaruction (a: Address) =
        this.Instructions[int a]

    member this.GetVideo: Value array =
        Array.sub this.Data 0x4000 8192

    member this.SetKey (k: int16) =
        this.Data[0x6000] <- k

    member this.Clean() =
        Array.Clear this.Data
        Array.Clear this.Instructions

    member this.Load(rom: uint16 array) =
        Array.Copy( rom, this.Instructions, rom.Length)

let Init: State = {
    Data = Array.create MemorySize 0s;
    Instructions = Array.create MemorySize 0us;
}

