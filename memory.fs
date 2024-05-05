module memory

open System

type Instruction = uint16
type Value = int16
type Address = uint16

let MemorySize = 2. **16 |> int

let videoMemBegin = 0x4000us
let videoMemLength = 8192us
let videoMemEnd = videoMemBegin + videoMemLength

type State = 
    {
    Data: Value array
    Instructions: Instruction array
    VideoUpdater: (Address -> Value -> unit) option
    }
    member this.SetMem (a: Address) (v: Value):unit = 
        this.Data[int a] <- v
        match this.VideoUpdater with
        | Some op when a >= videoMemBegin && a < videoMemEnd  -> op (a - videoMemBegin |> uint16) v
        | _ -> ()

    member this.GetMem (a: Address): Value = this.Data[int a]

    member this.GetInstaruction (a: Address) =
        this.Instructions[int a]

    member this.GetVideo: Value array =
        Array.sub this.Data (videoMemBegin |> int) (videoMemLength |> int)

    member this.SetKey (k: int16) =
        this.Data[0x6000] <- k

    member this.Clean() =
        Array.Clear this.Data
        Array.Clear this.Instructions

    member this.Load(rom: uint16 array) =
        Array.Copy( rom, this.Instructions, rom.Length)

let Init (display: (Address -> Value -> unit) option): State = {
    Data = Array.create MemorySize 0s;
    Instructions = Array.create MemorySize 0us;
    VideoUpdater = display
}

